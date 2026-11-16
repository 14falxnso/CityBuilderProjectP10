package com.thunderstar.city_builder;

import com.thunderstar.city_builder.config.Config;
import com.thunderstar.city_builder.database.Database;

public class Main {
    public static void main(String[] args) throws Exception {
        Config.init();
        Database.init();

        System.out.println("Hello World!");
    }
}
