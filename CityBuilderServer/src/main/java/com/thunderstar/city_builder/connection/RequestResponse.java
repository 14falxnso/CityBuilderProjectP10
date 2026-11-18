package com.thunderstar.city_builder.connection;

import com.google.gson.*;
import com.sun.net.httpserver.HttpExchange;
import com.thunderstar.city_builder.config.Config;
import com.thunderstar.city_builder.ui.GuiApp;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;
import java.util.function.Function;

import static com.thunderstar.city_builder.connection.EventStream.server;

public class RequestResponse {
    private static final Gson gson = new Gson();

    private static final Map<String, Function<JsonObject, JsonObject>> handlers = new HashMap<>();

    public static void init() {
        server.createContext(Config.data.path + "/requests", RequestResponse::handleRequests);

        GuiApp.instance.log("HTTP POST endpoint running on http://localhost:" + Config.data.port + Config.data.path + "/requests");
    }

    private static void handleRequests(HttpExchange ex) throws IOException {
        ex.getResponseHeaders().add("Access-Control-Allow-Origin", "*");

        try (ex) {
            byte[] bytes = ex.getRequestBody().readAllBytes();

            System.out.println(new String(bytes));

            if (bytes.length > 1024 * 1024) {
                send(ex, 413, error("Request too large"));
                return;
            }

            JsonObject request;

            try {
                JsonElement parsed = JsonParser.parseString(
                        new String(bytes, StandardCharsets.UTF_8));

                if (!parsed.isJsonObject()) {
                    send(ex, 400, error("Expected JSON object"));
                    return;
                }

                request = parsed.getAsJsonObject();
            } catch (JsonParseException e) {
                send(ex, 400, error("Invalid JSON"));
                return;
            }

            if (!request.has("type") ||
                    !request.get("type").isJsonPrimitive() ||
                    !request.getAsJsonPrimitive("type").isString() ||
                    !request.has("data") ||
                    !request.get("data").isJsonObject()) {
                send(ex, 400, error("Invalid request format"));
                return;
            }

            String type = request.get("type").getAsString();
            JsonObject data = request.getAsJsonObject("data");

            try {
                JsonObject response = handlers.get(type).apply(data);

                response.addProperty("success", true);

                send(ex, 200, response);
            } catch (Exception e) {
                send(ex, 500, error("Handler failed"));
            }
        }
    }

    private static void send(HttpExchange ex, int status, JsonObject response) throws IOException {
        byte[] bytes = gson.toJson(response).getBytes(StandardCharsets.UTF_8);

        ex.getResponseHeaders()
                .set("Content-Type", "application/json; charset=UTF-8");

        ex.sendResponseHeaders(status, bytes.length);

        ex.getResponseBody().write(bytes);
    }

    private static JsonObject error(String msg) {
        JsonObject response = new JsonObject();
        response.addProperty("success", false);
        response.addProperty("error", msg);
        return response;
    }

    public static void RegisterResponse(String type, Function<JsonObject, JsonObject> handler) {
        handlers.put(type, handler);

        GuiApp.instance.log("Registered request handler to request type: " + type);
    }
}
