package com.thunderstar.city_builder.connection;

import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpServer;
import com.thunderstar.city_builder.config.Config;
import com.thunderstar.city_builder.ui.GuiApp;
import com.thunderstar.city_builder.util.Threading;

import java.io.IOException;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.nio.charset.StandardCharsets;
import java.security.*;
import java.security.cert.CertificateException;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Executors;

public class EventStream {
    static HttpServer server;
    private static final Set<OutputStream> clients = ConcurrentHashMap.newKeySet();

    public static void init() throws IOException, NoSuchAlgorithmException, NoSuchProviderException, KeyStoreException, CertificateException, UnrecoverableKeyException, KeyManagementException {
        Runtime.getRuntime().addShutdownHook(new Thread(EventStream::shutdown));

        server = HttpServer.create(new InetSocketAddress("0.0.0.0", Config.data.port), 0);

        server.createContext(Config.data.path + "/events", EventStream::handleEvents);

        server.setExecutor(Executors.newCachedThreadPool());
        server.start();

        GuiApp.instance.log("SSE running on http://localhost:" + Config.data.port + Config.data.path + "/events");
    }

    private static void shutdown() {
        broadcastRaw("bye", "{}");

        if (server != null) {
            server.stop(0);
            server = null;
        }
    }

    private static void handleEvents(HttpExchange ex) {
        try {
            ex.getResponseHeaders().add("Content-Type", "text/event-stream");
            ex.getResponseHeaders().add("Cache-Control", "no-cache");
            ex.getResponseHeaders().add("Connection", "keep-alive");
            ex.getResponseHeaders().add("Access-Control-Allow-Origin", "*");

            ex.sendResponseHeaders(200, 0);

            OutputStream os = ex.getResponseBody();
            clients.add(os);

            os.write("".getBytes(StandardCharsets.UTF_8));

            GuiApp.instance.log("Client connected (" + clients.size() + ")");

            sendRawToOne(os, "hello", "{\"ok\":true}");

            while (true) {
                Thread.sleep(60_000);
                os.flush();
            }
        } catch (Exception e) {
            ex.close();
            clients.removeIf(os -> os == ex.getResponseBody());
            GuiApp.instance.log("Client disconnected (" + clients.size() + ")");
        }
    }

    public static void broadcastRaw(String event, String data) {
        Threading.SubmitTask(() -> {
            String payload = "event: " + event + "\n" +
                    "data: " + data + "\n\n";

            byte[] bytes = payload.getBytes(StandardCharsets.UTF_8);

            clients.removeIf(os -> {
                try {
                    os.write(bytes);
                    os.flush();
                    return false;
                } catch (Exception e) {
                    return true;
                }
            });

            return null;
        });
    }

    private static void sendRawToOne(OutputStream os, String event, String data) {
        Threading.SubmitTask(() -> {
            try {
                String payload = "event: " + event + "\n" +
                        "data: " + data + "\n\n";

                os.write(payload.getBytes(StandardCharsets.UTF_8));
                os.flush();
            } catch (Exception ignored) {}

            return null;
        });
    }
}
