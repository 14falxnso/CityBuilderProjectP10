package com.thunderstar.city_builder.config;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

import java.io.*;

public class Config {
    private static final Gson gson = new GsonBuilder().setPrettyPrinting().create();

    public static ConfigData data;

    public static void init() throws IOException {
        File config_file = new File("./data/config.json");

        try {
            data = gson.fromJson(new FileReader(config_file), ConfigData.class);
        } catch (Exception e) {
            data = new ConfigData();
        }

        if (!config_file.exists()) {
            new File(config_file.getParent()).mkdirs();
            config_file.createNewFile();
        }

        FileWriter config_writer = new FileWriter(config_file);

        config_writer.write(gson.toJson(data));
        config_writer.close();
    }

    public static class ConfigData {
        public int magicInt = 42;
        public int threads = Runtime.getRuntime().availableProcessors();
        public int port = 25565;
        public String path = "/city_builder";
    }
}
