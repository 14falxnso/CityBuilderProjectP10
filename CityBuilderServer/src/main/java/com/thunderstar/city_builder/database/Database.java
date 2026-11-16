package com.thunderstar.city_builder.database;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

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

    public static List<String> getAllTableNames() throws SQLException {
        List<String> tables = new ArrayList<>();

        Connection conn = DriverManager.getConnection(URL);
        DatabaseMetaData meta = conn.getMetaData();
        ResultSet rs = meta.getTables(null, null, "%", new String[]{"TABLE"});

        while (rs.next()) {
            tables.add(rs.getString("TABLE_NAME"));
        }

        rs.close();
        conn.close();

        return tables;
    }
}
