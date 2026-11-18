package com.thunderstar.city_builder.ui;

import com.thunderstar.city_builder.database.Database;
import com.thunderstar.city_builder.util.CommandParser;
import com.thunderstar.city_builder.util.NaturalSort;

import javax.swing.*;
import javax.swing.table.DefaultTableModel;
import javax.swing.table.TableModel;
import javax.swing.table.TableRowSorter;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.sql.*;
import java.time.LocalTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.function.BiConsumer;

public class GuiApp extends JFrame {
    public static GuiApp instance;

    private final JTextArea commandArea = new JTextArea();
    private final JTextField commandField = new JTextField();
    private final JTextField searchField = new JTextField();

    private final JTextArea logArea = new JTextArea();

    private final JTable tableArea = new JTable();
    private final JComboBox<String> tableDropdown = new JComboBox<>();
    private final JComboBox<String> searchDropdown = new JComboBox<>();

    private final JProgressBar progressBar = new JProgressBar(0, 1000);

    private BiConsumer<String, List<String>> onCommand;

    private List<String> commandHistory = new ArrayList<>();


    private static final Font defaultGuiFont = new Font("Consolas", Font.BOLD, 14);


    public GuiApp() {
        super("Citybuilder Server (Java)");

        setDefaultCloseOperation(EXIT_ON_CLOSE);
        setMinimumSize(new Dimension(900, 500));
        setLocationRelativeTo(null);


        buildUi();
        setVisible(true);

        try {
            updateDatabaseTable();
        } catch (SQLException e) {
            log(e.toString());
        }
    }


    private void buildUi() {
        commandArea.setEditable(false);
        commandArea.setLineWrap(false);
        commandArea.setFont(defaultGuiFont);

        logArea.setEditable(false);
        logArea.setLineWrap(false);
        logArea.setFont(defaultGuiFont);

        commandField.setFont(defaultGuiFont);

        searchField.setFont(defaultGuiFont);


        commandField.addActionListener(e -> submitCommand());

        InputMap im = commandField.getInputMap(JComponent.WHEN_FOCUSED);
        ActionMap am = commandField.getActionMap();


        im.put(KeyStroke.getKeyStroke("UP"), "historyUp");
        im.put(KeyStroke.getKeyStroke("DOWN"), "historyDown");

        am.put("historyUp", new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent e) {
                int index = commandHistory.indexOf(commandField.getText().trim());
                if (index >= commandHistory.size() - 1) return;
                commandField.setText(commandHistory.get(index + 1));
            }
        });

        am.put("historyDown", new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent e) {
                int index = commandHistory.indexOf(commandField.getText().trim());
                if (index < 0) return;
                if (index == 0) {
                    commandField.setText("");
                    return;
                }
                commandField.setText(commandHistory.get(index - 1));
            }
        });


        tableDropdown.addActionListener(e -> {
            String selected = (String) tableDropdown.getSelectedItem();
            if (selected == null) return;

            try {
                openDatabaseTable(selected);
            } catch (SQLException ex) {
                log(ex.toString());
            }
        });

        searchDropdown.addActionListener(e -> {
            String selected = (String) tableDropdown.getSelectedItem();
            if (selected == null) return;

            try {
                openDatabaseTable(selected);
            } catch (SQLException ex) {
                log(ex.toString());
            }
        });

        searchField.addActionListener(e -> {
            String selected = (String) tableDropdown.getSelectedItem();
            if (selected == null) return;

            try {
                openDatabaseTable(selected);
            } catch (SQLException ex) {
                log(ex.toString());
            }
        });


        JPanel leftPanel = new JPanel(new BorderLayout(8, 8));
        leftPanel.setBorder(BorderFactory.createTitledBorder("Commands"));
        leftPanel.add(new JScrollPane(commandArea), BorderLayout.CENTER);
        leftPanel.add(commandField, BorderLayout.SOUTH);


        JPanel rightPanel = new JPanel(new BorderLayout());

        JPanel rightTopPanel = new JPanel(new BorderLayout());
        rightTopPanel.setBorder(BorderFactory.createTitledBorder("Logs"));

        progressBar.setVisible(false);
        progressBar.setStringPainted(true);
        progressBar.setPreferredSize(new Dimension(10, 24));

        searchDropdown.setPreferredSize(new Dimension(128, 24));

        rightTopPanel.add(progressBar, BorderLayout.NORTH);
        rightTopPanel.add(new JScrollPane(logArea), BorderLayout.CENTER);

        JPanel rightBottomSearchPanel = new JPanel(new BorderLayout());
        rightBottomSearchPanel.add(searchDropdown, BorderLayout.WEST);
        rightBottomSearchPanel.add(searchField, BorderLayout.CENTER);

        JPanel rightBottomTopPanel = new JPanel(new BorderLayout());
        rightBottomTopPanel.add(tableDropdown, BorderLayout.NORTH);
        rightBottomTopPanel.add(rightBottomSearchPanel, BorderLayout.CENTER);

        JPanel rightBottomPanel = new JPanel(new BorderLayout());
        rightBottomPanel.setBorder(BorderFactory.createTitledBorder("SQL Table"));
        rightBottomPanel.add(new JScrollPane(tableArea), BorderLayout.CENTER);
        rightBottomPanel.add(rightBottomTopPanel, BorderLayout.NORTH);

        JSplitPane splitPane1 = new JSplitPane(JSplitPane.VERTICAL_SPLIT, rightTopPanel, rightBottomPanel);
        splitPane1.setResizeWeight(0.60);
        splitPane1.setDividerLocation(0.60);

        rightPanel.add(splitPane1);


        JSplitPane splitPane2 = new JSplitPane(JSplitPane.HORIZONTAL_SPLIT, leftPanel, rightPanel);
        splitPane2.setResizeWeight(0.40);
        splitPane2.setDividerLocation(0.40);

        setLayout(new BorderLayout());
        add(splitPane2, BorderLayout.CENTER);
    }


    private void submitCommand() {
        String raw = commandField.getText().trim();
        if (raw.isEmpty()) return;

        commandHistory.remove(raw);
        List<String> reversedCommandHistory = commandHistory.reversed();
        reversedCommandHistory.add(raw);
        commandHistory = reversedCommandHistory.reversed();

        List<String> parsed = CommandParser.parse(raw);

        String cmd = parsed.getFirst();
        List<String> args = parsed.subList(1, parsed.size());

        commandField.setText("");
        command("> " + raw);

        if (onCommand != null) {
            onCommand.accept(cmd, args);
        } else {
            command("(No command handler attached)");
        }
    }


    public void setOnCommand(BiConsumer<String, List<String>> handler) {
        this.onCommand = handler;
    }


    public void command(String msg) {
        SwingUtilities.invokeLater(() -> {
            commandArea.append(msg + "\n");
            commandArea.setCaretPosition(commandArea.getDocument().getLength());
        });
    }

    public void log(String msg) {
        SwingUtilities.invokeLater(() -> {
            logArea.append(time() + " - " + msg + "\n");
            logArea.setCaretPosition(logArea.getDocument().getLength());
        });
    }

    public void logStackTrace(Throwable e) {
        log(e.toString());
        for (StackTraceElement element : e.getStackTrace()) {
            log("    at " + element.toString());
        }
    }

    public void setProgress(double progress, String header) {
        SwingUtilities.invokeLater(() -> {
            progressBar.setVisible(true);

            double clampedProgress = Math.clamp(progress, 0.0, 1.0);

            progressBar.setValue((int) (clampedProgress * 1000));
            progressBar.setString(header + " (" + (int) (clampedProgress * 100) + "." + ((int)(clampedProgress * 1000))%10 + "%)");
        });
    }

    public void hideProgress() {
        SwingUtilities.invokeLater(() -> progressBar.setVisible(false));
    }

    public void clearAll() {
        SwingUtilities.invokeLater(() -> {
            commandArea.setText("");
            logArea.setText("");
        });
    }


    public void updateDatabaseTable() throws SQLException {
        String current = (String) tableDropdown.getSelectedItem();

        tableDropdown.removeAllItems();
        for (String t : Database.getAllTableNames()) {
            tableDropdown.addItem(t);
        }

        for (int i = 0; i < tableDropdown.getItemCount(); i++) {
            if (Objects.equals(current, tableDropdown.getItemAt(i))) tableDropdown.setSelectedIndex(i);
        }
    }

    public void openDatabaseTable(String table) throws SQLException {
        Connection conn = DriverManager.getConnection(Database.URL);
        Statement stmt = conn.createStatement();
        ResultSet result = stmt.executeQuery("SELECT * FROM " + table);

        ResultSetMetaData meta = result.getMetaData();
        int colCount = meta.getColumnCount();

        List<String> columnList = new ArrayList<>();
        List<Object[]> dataList = new ArrayList<>();

        for (int i = 1; i <= colCount; i++) {
            columnList.add(meta.getColumnName(i));
        }

        int searchDropdownIndex = searchDropdown.getSelectedIndex();

        searchDropdown.removeAllItems();
        for (String col : columnList) {
            searchDropdown.addItem(col);
        }

        searchDropdown.setSelectedIndex(searchDropdownIndex);

        result = stmt.executeQuery("SELECT * FROM " + table + " WHERE CAST(" + searchDropdown.getSelectedItem() + " AS TEXT) LIKE '%" + searchField.getText() + "%'");

        while (result.next()) {
            Object[] row = new Object[colCount];
            for (int i = 1; i <= colCount; i++) {
                row[i - 1] = result.getObject(i);
            }
            dataList.add(row);
        }

        String[] columns = columnList.toArray(new String[0]);
        Object[][] data = dataList.toArray(new Object[0][]);

        DefaultTableModel model = new DefaultTableModel(data, columns) {
            @Override
            public boolean isCellEditable(int row, int column) { return false; }
        };

        List<? extends RowSorter.SortKey> oldSortKeys = null;
        RowSorter<? extends TableModel> oldSorter = tableArea.getRowSorter();
        if (oldSorter != null) {
            oldSortKeys = oldSorter.getSortKeys();
        }

        tableArea.setModel(model);

        TableRowSorter<TableModel> sorter = new TableRowSorter<>(model);

        for (int i = 0; i < colCount; i++) {
            sorter.setComparator(i, NaturalSort.comparator());
        }

        if (oldSortKeys != null) {
            try {
                sorter.setSortKeys(oldSortKeys);
            } catch (Exception e) {
                log(e.toString());
            }
        }

        tableArea.setRowSorter(sorter);

        result.close();
        stmt.close();
        conn.close();
    }


    private static String time() {
        return LocalTime.now().format(DateTimeFormatter.ofPattern("HH:mm:ss"));
    }
}