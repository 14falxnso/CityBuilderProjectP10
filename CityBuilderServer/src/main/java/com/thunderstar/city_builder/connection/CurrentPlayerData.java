package com.thunderstar.city_builder.connection;

import com.google.gson.JsonObject;

import java.math.BigInteger;
import java.nio.Buffer;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;
import java.util.*;

public class CurrentPlayerData {
    private static final SecureRandom rand = new SecureRandom();

    private static final byte[] dhPrime = HexFormat.of().parseHex("FFFFFFFFFFFFFFFFADF85458A2BB4A9AAFDC5620273D3CF1D8B9C583CE2D3695A9E13641146433FBCC939DCE249B3EF97D2FE363630C75D8F681B202AEC4617AD3DF1ED5D5FD65612433F51F5F066ED0856365553DED1AF3B557135E7F57C935984F0C70E0E68B77E2A689DAF3EFE8721DF158A136ADE73530ACCA4F483A797ABC0AB182B324FB61D108A94BB2C8E3FBB96ADAB760D7F4681D4F42A3DE394DF4AE56EDE76372BB190B07A7C8EE0A6D709E02FCE1CDF7E2ECC03404CD28342F619172FE9CE98583FF8E4F1232EEF28183C3FE3B1B4C6FAD733BB5FCBC2EC22005C58EF1837D1683B2C6F34A26C1B2EFFA886B423861285C97FFFFFFFFFFFFFFFF");
    private static final byte[] dhGenerator = { 0x02 };

    private static final Map<Integer, String> keyMap = new HashMap<>();

    public static JsonObject GetKeyRequest(JsonObject request) {
        int key_id = request.get("key_id").getAsInt();

        byte[] client_public_key = Base64.getDecoder().decode(request.get("public_key").getAsString());

        JsonObject response = new JsonObject();

        if (keyMap.containsKey(key_id)) {
            response.addProperty("success", false);
        } else {
            response.addProperty("success", true);

            byte[] private_key = new byte[32];
            rand.nextBytes(private_key);

            byte[] server_public_key = ModPow(dhGenerator, private_key, dhPrime, 256);

            byte[] shared_secret = ModPow(client_public_key, private_key, dhPrime, 256);

            MessageDigest sha = null;
            try {
                sha = MessageDigest.getInstance("SHA-256");
            } catch (NoSuchAlgorithmException e) {
                response.remove("success");
                response.addProperty("success", false);

                return response;
            }

            keyMap.put(key_id, Base64.getEncoder().encodeToString(sha.digest(shared_secret)));

            response.addProperty("public_key", Base64.getEncoder().encodeToString(server_public_key));
        }

        return response;
    }

    public static String GetKeyById(int keyId) {
        return keyMap.remove(keyId);
    }


    private static byte[] ModPow(byte[] base, byte[] power, byte[] mod, int outputBytes) {
        BigInteger baseValue = new BigInteger(1, base);
        BigInteger powerValue = new BigInteger(1, power);
        BigInteger modValue = new BigInteger(1, mod);

        BigInteger result = baseValue.modPow(powerValue, modValue);

        byte[] raw = result.toByteArray();
        byte[] output = new byte[outputBytes];

        int copyLength = Math.min(raw.length, outputBytes);
        System.arraycopy(raw, raw.length - copyLength, output, outputBytes - copyLength, copyLength);

        return output;
    }
}
