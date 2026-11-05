using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;

namespace DeskDock;

// Una sola superficie vectorial: no crea botones, temporizadores ni animaciones por etapa.
public sealed class PomodoroStagesView : FrameworkElement
{
    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(nameof(Accent), typeof(Brush), typeof(PomodoroStagesView),
        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty MutedProperty = DependencyProperty.Register(nameof(Muted), typeof(Brush), typeof(PomodoroStagesView),
        new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));
    public Brush Accent { get => (Brush)GetValue(AccentProperty); set => SetValue(AccentProperty, value); }
    public Brush Muted { get => (Brush)GetValue(MutedProperty); set => SetValue(MutedProperty, value); }
    public bool Compact { get; set; }
    private PomodoroTimer? _timer;
    private int _revision = -1, _rounds = 4, _current;
    private PomodoroStageResult[] _results = Array.Empty<PomodoroStageResult>();
    private readonly List<int?> _indices = new();
    internal IReadOnlyList<int?> VisibleStageIndices => _indices;
    internal int CurrentStageIndex => _current;

    public PomodoroStagesView()
    {
        SetResourceReference(AccentProperty, "AccentBrush");
        SetResourceReference(MutedProperty, "TextMuted");
        Focusable = false;
    }

    internal void SetTimer(PomodoroTimer timer)
    {
        if (ReferenceEquals(_timer, timer) && _revision == timer.Revision) return;
        _timer = timer; _revision = timer.Revision;
        _rounds = timer.Options.SessionsBeforeLongBreak; _current = timer.CycleStageIndex;
        _results = timer.CycleResults.ToArray();
        _indices.Clear();
        int total = _rounds * 2;
        int start = Compact && total > 8 ? Math.Clamp(_current - 2, 0, total - 5) : 0;
        int end = Compact && total > 8 ? start + 5 : total;
        if (start > 0) _indices.Add(null);
        for (int i = start; i < end; i++) _indices.Add(i);
        if (end < total) _indices.Add(null);
        var description = new List<string>();
        for (int i = 0; i < total; i++)
            description.Add($"{PomodoroTimer.StageName(StageAt(i))} {i / 2 + 1}/{_rounds}: {StatusAt(i)}");
        string text = string.Join(Environment.NewLine, description);
        ToolTip = text;
        AutomationProperties.SetName(this, text);
        InvalidateMeasure(); InvalidateVisual();
    }

    private PomodoroStage StageAt(int index) => index == _rounds * 2 - 1 ? PomodoroStage.LongBreak :
        index % 2 == 0 ? PomodoroStage.Focus : PomodoroStage.ShortBreak;
    private string StatusAt(int index) => index == _current ? "Actual" : index >= _results.Length ? "Pendiente" :
        _results[index] == PomodoroStageResult.Skipped ? "Saltada" : "Terminada";
    private int Columns(double available) => Compact ? Math.Max(1, _indices.Count) :
        Math.Max(1, Math.Min(12, Math.Min(_indices.Count, double.IsInfinity(available) ? 12 : (int)(available / 18))));

    protected override Size MeasureOverride(Size availableSize)
    {
        int columns = Columns(availableSize.Width);
        int rows = Math.Max(1, (_indices.Count + columns - 1) / columns);
        return new Size(columns * (Compact ? 12 : 18), rows * (Compact ? 14 : 22));
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        int columns = Columns(ActualWidth);
        double pitch = Compact ? 12 : 18, rowHeight = Compact ? 14 : 22;
        double size = Compact ? 7 : 11, radius = size / 2;
        for (int slot = 0; slot < _indices.Count; slot++)
        {
            double x = slot % columns * pitch + pitch / 2, y = slot / columns * rowHeight + radius + 1;
            if (_indices[slot] is not int index)
            {
                for (int dot = -1; dot <= 1; dot++) dc.DrawEllipse(Muted, null, new Point(x + dot * 3, y), 0.75, 0.75);
                continue;
            }
            bool current = index == _current;
            bool completed = index < _results.Length && _results[index] == PomodoroStageResult.Completed;
            bool skipped = index < _results.Length && _results[index] == PomodoroStageResult.Skipped;
            var pen = new Pen(current || completed || skipped ? Accent : Muted, current ? 1.5 : 1);
            Brush? fill = completed ? Accent : null;
            var stage = StageAt(index);
            if (stage == PomodoroStage.Focus) dc.DrawEllipse(fill, pen, new Point(x, y), radius, radius);
            else
            {
                var shape = new StreamGeometry();
                using (var path = shape.Open())
                {
                    path.BeginFigure(new Point(x, y - radius), true, true);
                    if (stage == PomodoroStage.LongBreak)
                    {
                        path.LineTo(new Point(x + radius, y), true, false);
                        path.LineTo(new Point(x, y + radius), true, false);
                        path.LineTo(new Point(x - radius, y), true, false);
                    }
                    else
                    {
                        path.LineTo(new Point(x + radius, y + radius), true, false);
                        path.LineTo(new Point(x - radius, y + radius), true, false);
                    }
                }
                shape.Freeze(); dc.DrawGeometry(fill, pen, shape);
            }
            if (skipped) dc.DrawLine(new Pen(Accent, 1), new Point(x - radius * .5, y + 1), new Point(x + radius * .5, y + 1));
            if (current) dc.DrawEllipse(Accent, null, new Point(x, y + radius + 3), Compact ? 1 : 1.4, Compact ? 1 : 1.4);
        }
    }
}
