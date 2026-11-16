using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;

namespace SchoolDatabaseDemo
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=school.db";

        public MainWindow()
        {
            InitializeComponent();
        }

        private SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        private void CreateDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Students (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        ClassName TEXT NOT NULL,
                        AverageGrade REAL
                    );";
                command.ExecuteNonQuery();
                StatusTextBlock.Text = "Базата е готова.";
                LoadStudents();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Грешка: " + ex.Message;
            }
        }

        private void LoadStudents()
        {
            try
            {
                var students = new List<Student>();
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, ClassName, AverageGrade FROM Students ORDER BY Id";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(new Student
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        ClassName = reader.GetString(2),
                        AverageGrade = reader.GetDouble(3)
                    });
                }
                StudentsGrid.ItemsSource = students;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Грешка при зареждане: " + ex.Message;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameBox.Text) || !double.TryParse(GradeBox.Text, out double grade))
                {
                    StatusTextBlock.Text = "Невалидни данни.";
                    return;
                }

                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Students (Name, ClassName, AverageGrade) VALUES ($name, $class, $grade)";
                command.Parameters.AddWithValue("$name", NameBox.Text);
                command.Parameters.AddWithValue("$class", ClassBox.Text);
                command.Parameters.AddWithValue("$grade", grade);

                command.ExecuteNonQuery();
                StatusTextBlock.Text = "Ученикът е добавен.";
                LoadStudents();
                ClearFields();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Грешка: " + ex.Message;
            }
        }

        private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var student = StudentsGrid.SelectedItem as Student;
            if (student != null)
            {
                NameBox.Text = student.Name;
                ClassBox.Text = student.ClassName;
                GradeBox.Text = student.AverageGrade.ToString();
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var student = StudentsGrid.SelectedItem as Student;
                if (student == null) return;

                if (!double.TryParse(GradeBox.Text, out double grade)) return;

                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "UPDATE Students SET Name = $name, ClassName = $class, AverageGrade = $grade WHERE Id = $id";
                command.Parameters.AddWithValue("$name", NameBox.Text);
                command.Parameters.AddWithValue("$class", ClassBox.Text);
                command.Parameters.AddWithValue("$grade", grade);
                command.Parameters.AddWithValue("$id", student.Id);

                command.ExecuteNonQuery();
                StatusTextBlock.Text = "Записът е обновен.";
                LoadStudents();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Грешка: " + ex.Message;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var student = StudentsGrid.SelectedItem as Student;
                if (student == null) return;

                var result = MessageBox.Show("Сигурни ли сте, че искате да изтриете този запис?", "Потвърждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using var connection = OpenConnection();
                    using var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Students WHERE Id = $id";
                    command.Parameters.AddWithValue("$id", student.Id);

                    command.ExecuteNonQuery();
                    StatusTextBlock.Text = "Записът е изтрит.";
                    LoadStudents();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Грешка: " + ex.Message;
            }
        }

        private void ClearFields()
        {
            NameBox.Clear();
            ClassBox.Clear();
            GradeBox.Clear();
            StudentsGrid.SelectedItem = null;
        }
    }
}