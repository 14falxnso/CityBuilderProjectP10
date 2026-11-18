package com.thunderstar.city_builder.util;

import com.thunderstar.city_builder.ui.GuiApp;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.Consumer;

public class Commands {
    public static void commands(String cmd, List<String> args) {
        Command command = commands.get(cmd);

        if (command == null) {
            GuiApp.instance.command("invalid command: " + cmd);
            return;
        }

        command.run(args);
    }


    public static Map<String, Command> commands = new HashMap<>();


    public static void init() {
        new Command("help", 0, args -> {
            GuiApp.instance.command("Commands:");

            commands.values().stream().flatMap(v -> v.descriptors.stream())
                    .sorted()
                    .forEach(desc -> GuiApp.instance.command("  " + desc));
        });

        new Command("echo", 1, args -> GuiApp.instance.log(args.getFirst()), "\"text\"");
    }


    public static class Command {
        private final String name;
        private final int minArgs;
        private final Consumer<List<String>> func;
        public final List<String> descriptors = new ArrayList<>();

        public Command(String name, int minArgs, Consumer<List<String>> func) {
            this.name = name;
            this.minArgs = minArgs;
            this.func = func;
            this.descriptors.add(name);

            commands.put(name, this);
        }

        public Command(String name, int minArgs, Consumer<List<String>> func, String descriptor) {
            this.name = name;
            this.minArgs = minArgs;
            this.func = func;
            this.descriptors.add(name + " " + descriptor);

            commands.put(name, this);
        }

        public Command(String name, int minArgs, Consumer<List<String>> func, List<String> descriptors) {
            this.name = name;
            this.minArgs = minArgs;
            this.func = func;
            for (String descriptor : descriptors) this.descriptors.add(name + " " + descriptor);

            commands.put(name, this);
        }

        public void run(List<String> args) {
            if (args.size() < minArgs) {
                GuiApp.instance.command(name + " requires at least " + minArgs + " argument(s)");
                return;
            }

            func.accept(args);
        }
    }
}
