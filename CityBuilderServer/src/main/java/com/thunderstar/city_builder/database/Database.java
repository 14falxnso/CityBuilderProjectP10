package com.thunderstar.city_builder.database;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.sql.Statement;

public class Database {
    public static final String URL = "jdbc:sqlite:data/database.db";

    public static void init() throws SQLException {
        Connection conn = DriverManager.getConnection(URL);
        Statement stmt = conn.createStatement();

        stmt.execute("CREATE TABLE IF NOT EXISTS data (" +
                "id INTEGER PRIMARY KEY, email TEXT NOT NULL UNIQUE, password TEXT NOT NULL, " +
                "username TEXT NOT NULL, tileData TEXT NOT NULL, resourceData TEXT NOT NULL)");

        stmt.close();
        conn.close();
    }
}
