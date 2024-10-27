using LAB1.Parser;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Grid = Microsoft.Maui.Controls.Grid;

namespace LAB1
{
    public partial class MainPage : ContentPage
    {
        private const int start_column_count = 10;
        private const int start_row_count = 20;

        private Entry last_clicked_cell = new Entry();

        private Dictionary<string, string> cell_expressions = new Dictionary<string, string>();

        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
        }

        private void CreateGrid()
        {
            AddColumnsAndColumnLabels();
            AddRowsAndCellEntries();
        }

        private void AddColumnsAndColumnLabels()
        {
            for (int col = 0; col < start_column_count + 1; col++)
            {
                grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition());
                
                if (col > 0)
                {
                    var label = new Label
                    {
                        Text = "A" + col.ToString(),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, col);
                    grid.Children.Add(label);
                }
            }
        }
        private void AddRowsAndCellEntries()
        {
            for (int row = 0; row < start_row_count + 1; row++)
            {
                grid.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition());
                
                if (row > 0)
                {
                    CreateAndSetRowLabel(row);

                    for (int col = 0; col < start_column_count; col++)
                    {
                        CreateAndSetCell(row, col);
                    }
                }
            }
        }

        private void SaveButtonClicked(object sender, EventArgs e)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("GridData");

            int row_count = grid.RowDefinitions.Count;
            int col_count = grid.ColumnDefinitions.Count;

            for (int row = 0; row < row_count + 1; row++)
            {
                for (int col = 0; col < col_count + 1; col++)
                {
                    var entry = grid.Children.FirstOrDefault(e => grid.GetRow(e) == row && grid.GetColumn(e) == col) as Entry;

                    if (entry is not null)
                    {
                        worksheet.Cell(row, col).Value = entry.Text;
                    }
                }
            }

            string file_name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LAB1Grid.xlsx");

            workbook.SaveAs(file_name);

            DisplayAlert("Success", "ok", "ok");
        }

        private void CalculateButtonClicked(object sender, EventArgs e)
        {
            string expr = text_input.Text;

            Evaluator evaluator = new Evaluator(cell_expressions);

            try
            {
                Evaluator.HandleEmptyExpression(expr);

                var result = evaluator.Evaluate(expr);

                text_input.Text = result.ToString();
                last_clicked_cell.Text = result.ToString();
            }
            catch (Exception ex)
            {
                DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private async void ExitButtonClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти?", "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }

        private void AddRowButtonClicked(object sender, EventArgs e)
        {
            int new_row = grid.RowDefinitions.Count();

            grid.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition());

            CreateAndSetRowLabel(new_row);

            for (int col = 0; col < grid.ColumnDefinitions.Count; col++)
            {
                CreateAndSetCell(new_row, col);
            }
        }

        private void AddColumnButtonClicked(object sender, EventArgs e)
        {
            int new_col = grid.ColumnDefinitions.Count();

            grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition());

            var label = new Label
            {
                Text = "A" + new_col.ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, new_col);
            grid.Children.Add(label);

            for(int row = 1; row < grid.RowDefinitions.Count; row++)
            {
                CreateAndSetCell(row, new_col - 1);
            }
        }

        private void DeleteRowButtonClicked(object sender, EventArgs e)
        {
            if (grid.RowDefinitions.Count() > 2)
            {
                int last_row_index = grid.RowDefinitions.Count() - 1;
                grid.RowDefinitions.RemoveAt(last_row_index);

                var elements_to_remove = new List<Microsoft.Maui.IView>();
                
                foreach (var child in grid.Children)
                {
                    if (grid.GetRow(child) == last_row_index)
                    {
                        elements_to_remove.Add(child);
                    }
                }

                foreach (var element in elements_to_remove)
                {
                    grid.Children.Remove(element);
                }
            }
        }

        private void DeleteColumnButtonClicked(object sender, EventArgs e)
        {
            if (grid.ColumnDefinitions.Count() > 2)
            {
                int last_col_index = grid.ColumnDefinitions.Count() - 1;
                grid.ColumnDefinitions.RemoveAt(last_col_index);

                var elements_to_remove = new List<Microsoft.Maui.IView>();

                foreach (var child in grid.Children)
                {
                    if (grid.GetColumn(child) == last_col_index)
                    {
                        elements_to_remove.Add(child);
                    }
                }

                foreach (var element in elements_to_remove)
                {
                    grid.Children.Remove(element);
                }
            }
        }

        private async void HelpButtonClicked(object sender, EventArgs e)
        {
            string info = @"Лабораторна робота 1. Варіант 4
Автор: Бессарабов Володимир
                            
Інструкція:
    – Програма підтримує наступні операції: +, -, *, /, mod, div, mmax(), mmin() та посилання на інші клітинки у вигляді &ROWACOL(ROW - номер рядка, COL - номер стовпчика).
    – Перед тим, як уводити вираз у головний рядок, виберіть клітинку, у яку хочете вираз записати. Інакше вираз нікуди не запишеться.
    – Для коректної роботи програми потрібно правильно записувати вирази: без використання невизначених операторів, літер та інших символів.
    – Кнопка 'Зберегти' зберігає таблицю на робочий стіл у форматі .xlsx.
    - Кнопка 'Порахувати' обраховує вираз, що записаний в головному рядку(останній натиснутій клітинці).";       

            await DisplayAlert("Довідка", info, "OK");
        }

        private void CreateAndSetRowLabel(int row)
        {
            var label = new Label
            {
                Text = (row).ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);
        }

        private void CreateAndSetCell(int row, int col)
        {
            var entry = new Entry
            {
                Text = "",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            entry.TextChanged += EntryTextChanged;
            entry.Focused += EntryFocused;
            cell_expressions[row.ToString() + "A" + col.ToString()] = entry.Text;
            Grid.SetRow(entry, row);
            Grid.SetColumn(entry, col + 1);
            grid.Children.Add(entry);
        }

        private void EntryFocused(object sender, FocusEventArgs e)
        {
            last_clicked_cell = (Entry)sender;
            text_input.Text = last_clicked_cell.Text;
        }

        private void EntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == last_clicked_cell)
            {
                text_input.Text = e.NewTextValue;
                cell_expressions[Grid.GetRow(last_clicked_cell).ToString() + "A" + Grid.GetColumn(last_clicked_cell).ToString()] = text_input.Text;
            }
        }

        private void MainEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (last_clicked_cell is not null)
            {
                last_clicked_cell.Text = e.NewTextValue;
                cell_expressions[Grid.GetRow(last_clicked_cell).ToString() + "A" + Grid.GetColumn(last_clicked_cell).ToString()] = text_input.Text;
            }
        }
    }

}
