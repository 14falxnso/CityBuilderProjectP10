package com.thunderstar.city_builder.util;

import com.thunderstar.city_builder.config.Config;

import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

public class Threading {
    private static ExecutorService executor;

    public static void init() {
        executor = Executors.newFixedThreadPool(Config.data.threads);
    }

    public static <T> Future<T> SubmitTask(Callable<T> task) {
        return executor.submit(task);
    }

    public static ExecutorService GetExecutor() {
        return executor;
    }
}
