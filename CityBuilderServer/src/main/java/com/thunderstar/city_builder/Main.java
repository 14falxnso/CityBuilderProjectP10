package com.thunderstar.city_builder;

import com.google.gson.Gson;
import com.thunderstar.city_builder.config.Config;
import com.thunderstar.city_builder.connection.CurrentPlayerData;
import com.thunderstar.city_builder.connection.EventStream;
import com.thunderstar.city_builder.connection.RequestResponse;
import com.thunderstar.city_builder.database.Database;
import com.thunderstar.city_builder.ui.GuiApp;
import com.thunderstar.city_builder.util.Commands;
import com.thunderstar.city_builder.util.Threading;

import javax.swing.*;

public class Main {
    public static void main(String[] args) throws Exception {
        SwingUtilities.invokeLater(() -> {
            GuiApp.instance = new GuiApp();
            GuiApp.instance.setOnCommand(Commands::commands);

            try {
                Config.init();
                GuiApp.instance.log("Config loaded with Magic Int " + Config.data.magicInt + ".");

                Database.init();
                GuiApp.instance.log("Database initialized.");

                Commands.init();
                GuiApp.instance.log("commands loaded.");

                Threading.init();
                GuiApp.instance.log("Thread pool initialized.");

                EventStream.init();
                RequestResponse.init();
            } catch (Exception e) {
                GuiApp.instance.logStackTrace(e);
            }


            RequestResponse.RegisterResponse("test", false, obj -> {
                GuiApp.instance.log(new Gson().toJson(obj));
                return obj;
            });

            RequestResponse.RegisterResponse("getKey", false, CurrentPlayerData::GetKeyRequest);
        });
    }
}
