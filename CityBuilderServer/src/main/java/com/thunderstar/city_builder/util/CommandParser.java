package com.thunderstar.city_builder.util;

import java.util.ArrayList;
import java.util.List;

public class CommandParser {
    public static List<String> parse(String input) {
        List<String> tokens = new ArrayList<>();
        if (input == null) return tokens;

        String s = input.trim();
        if (s.isEmpty()) return tokens;

        StringBuilder current = new StringBuilder();
        boolean inQuotes = false;

        for (int i = 0; i < s.length(); i++) {
            char c = s.charAt(i);

            if (c == '"') {
                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes && Character.isWhitespace(c)) {
                if (!current.isEmpty()) {
                    tokens.add(current.toString());
                    current.setLength(0);
                }
                continue;
            }

            current.append(c);
        }

        if (!current.isEmpty()) {
            tokens.add(current.toString());
        }

        return tokens;
    }
}
