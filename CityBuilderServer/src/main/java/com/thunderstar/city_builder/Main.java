package com.thunderstar.city_builder;

import com.thunderstar.city_builder.config.Config;
import com.thunderstar.city_builder.database.Database;
import com.thunderstar.city_builder.ui.GuiApp;
import com.thunderstar.city_builder.util.Commands;

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

                Commands.Init();
                GuiApp.instance.log("commands loaded.");
            } catch (Exception e) {
                GuiApp.instance.log(e.toString());
                for (StackTraceElement element : e.getStackTrace()) {
                    GuiApp.instance.log("    at " + element.toString());
                }
            }
        });
    }
}
