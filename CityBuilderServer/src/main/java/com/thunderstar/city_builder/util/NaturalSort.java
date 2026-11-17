package com.thunderstar.city_builder.util;

import java.util.Comparator;

public class NaturalSort {
    public static Comparator<?> comparator() {
        return (c, d) -> {
            String a = c.toString();
            String b = d.toString();

            if (a == null) return (b == null) ? 0 : -1;
            if (b == null) return 1;

            int i = 0, j = 0;
            int n = a.length(), m = b.length();

            while (i < n && j < m) {
                char ca = a.charAt(i);
                char cb = b.charAt(j);

                if (Character.isDigit(ca) && Character.isDigit(cb)) {
                    int startI = i;
                    int startJ = j;

                    while (i < n && Character.isDigit(a.charAt(i))) i++;
                    while (j < m && Character.isDigit(b.charAt(j))) j++;

                    String numA = a.substring(startI, i);
                    String numB = b.substring(startJ, j);

                    if (numA.length() != numB.length()) {
                        return Integer.compare(numA.length(), numB.length());
                    }

                    int cmp = numA.compareTo(numB);
                    if (cmp != 0) return cmp;
                } else {
                    ca = Character.toLowerCase(ca);
                    cb = Character.toLowerCase(cb);

                    if (ca != cb) return Character.compare(ca, cb);
                    i++;
                    j++;
                }
            }

            return Integer.compare(n, m);
        };
    }
}
