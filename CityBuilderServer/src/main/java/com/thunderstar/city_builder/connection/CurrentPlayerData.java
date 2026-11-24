package com.thunderstar.city_builder.connection;

import com.google.gson.JsonObject;

import java.security.SecureRandom;
import java.util.Base64;
import java.util.HashMap;
import java.util.Map;

public class CurrentPlayerData {
    private static final SecureRandom rand = new SecureRandom();

    private static final Map<Integer, String> keyMap = new HashMap<>();

    public static JsonObject GetKeyRequest(JsonObject request) {
        int key_id = request.get("key_id").getAsInt();

        JsonObject response = new JsonObject();

        if (keyMap.containsKey(key_id)) {
            response.addProperty("success", false);
        } else {
            response.addProperty("success", true);

            byte[] key = new byte[32];
            rand.nextBytes(key);

            keyMap.put(key_id, Base64.getEncoder().encodeToString(key));
            response.addProperty("key", keyMap.get(key_id));
        }

        return response;
    }

    public static String GetKeyById(int keyId) {
        return keyMap.remove(keyId);
    }
}
