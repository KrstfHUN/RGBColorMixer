using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RGBColorMixer;

public partial class MainWindow : Window
{
    private bool _isDrawing = false;
    private Point _lastPoint;
    private readonly Random _rng = new();

    public MainWindow()
    {
        InitializeComponent();
        UpdateColor();
    }


    private (byte r, byte g, byte b) GetChannels()
    {
        byte r = (CbR.IsChecked == true) ? (byte)SliderR.Value : (byte)0;
        byte g = (CbG.IsChecked == true) ? (byte)SliderG.Value : (byte)0;
        byte b = (CbB.IsChecked == true) ? (byte)SliderB.Value : (byte)0;
        return (r, g, b);
    }

    private void UpdateColor()
    {
        if (ColorPreview is null) return;

        var (r, g, b) = GetChannels();
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        ColorPreview.Background = brush;
        TbRgb.Text = $"RGB: ({r}, {g}, {b})";
        TbHex.Text = $"HEX: #{r:X2}{g:X2}{b:X2}";

        double lum = 0.299 * r + 0.587 * g + 0.114 * b;
        PreviewLabel.Foreground = lum > 140 ? Brushes.Black : Brushes.White;
        PreviewLabel.Text = $"#{r:X2}{g:X2}{b:X2}";
    }


    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        => UpdateColor();

    private void Cb_Changed(object sender, RoutedEventArgs e)
        => UpdateColor();

    private void SliderBrush_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TbBrushSize is not null)
            TbBrushSize.Text = $"Méret: {(int)SliderBrush.Value} px";
    }


    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        SliderR.Value = SliderG.Value = SliderB.Value = 128;
        CbR.IsChecked = CbG.IsChecked = CbB.IsChecked = true;
        UpdateColor();
    }

    private void BtnRandom_Click(object sender, RoutedEventArgs e)
    {
        SliderR.Value = _rng.Next(256);
        SliderG.Value = _rng.Next(256);
        SliderB.Value = _rng.Next(256);
        UpdateColor();
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
        => DrawingCanvas.Children.Clear();


    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        _isDrawing = true;
        _lastPoint = e.GetPosition(DrawingCanvas);
        DrawingCanvas.CaptureMouse();
        DrawDot(_lastPoint);
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDrawing || e.LeftButton != MouseButtonState.Pressed) return;
        Point current = e.GetPosition(DrawingCanvas);
        DrawLine(_lastPoint, current);
        _lastPoint = current;
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDrawing = false;
        DrawingCanvas.ReleaseMouseCapture();
    }

    private void DrawDot(Point p)
    {
        double size = SliderBrush.Value;
        var (r, g, b) = GetChannels();
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromRgb(r, g, b))
        };
        Canvas.SetLeft(ellipse, p.X - size / 2);
        Canvas.SetTop(ellipse, p.Y - size / 2);
        DrawingCanvas.Children.Add(ellipse);
    }

    private void DrawLine(Point from, Point to)
    {
        double size = SliderBrush.Value;
        var (r, g, b) = GetChannels();
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));

        double dx = to.X - from.X;
        double dy = to.Y - from.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        int steps = Math.Max(1, (int)(dist / (size / 4)));

        for (int i = 0; i <= steps; i++)
        {
            double t = (double)i / steps;
            double x = from.X + dx * t;
            double y = from.Y + dy * t;

            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = brush
            };
            Canvas.SetLeft(ellipse, x - size / 2);
            Canvas.SetTop(ellipse, y - size / 2);
            DrawingCanvas.Children.Add(ellipse);
        }
    }
}
