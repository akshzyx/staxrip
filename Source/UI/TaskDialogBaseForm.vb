﻿
Imports System.Runtime.InteropServices

Imports StaxRip.UI

Public Class TaskDialogBaseForm
    Property Theme As Theme = ThemeManager.CurrentTheme

    Overridable Sub AdjustHeight()
    End Sub

    Public Sub New()
        InitializeComponent()
        AddHandler InputTextEdit.TextBox.KeyDown, AddressOf InputTextEditTextBoxKeyDown

        If Not DesignHelp.IsDesignMode Then
            BackColor = Theme.General.BackColor
            TitleLabel.ForeColor = Theme.General.Controls.Label.ForeColor
        End If
    End Sub

    <DllImport("user32.dll", EntryPoint:="SetWindowLong")>
    Shared Function SetWindowLong32(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongPtr")>
    Shared Function SetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
    End Function

    Shared Function SetWindowLongPtr(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
        If IntPtr.Size = 8 Then
            Return SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
        Else
            Return New IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32))
        End If
    End Function

    Class TaskDialogPanel
        Inherits Panel

        Property Form As TaskDialogBaseForm
        Property LineBreaks As Integer

        Protected Overrides Sub OnLayout(levent As LayoutEventArgs)
            MyBase.OnLayout(levent)

            If DesignMode OrElse Controls.Count = 0 Then
                MyBase.OnLayout(levent)
                Exit Sub
            End If

            If Form Is Nothing Then
                Form = DirectCast(FindForm(), TaskDialogBaseForm)
            End If

            Dim fh = FontHeight
            Dim previous As Control

            Using g = CreateGraphics()
                For x = 0 To Controls.Count - 1
                    Dim c = Controls(x)

                    If x = 0 Then
                        c.Top = 0
                    Else
                        c.Top = previous.Top + previous.Height + CInt(fh * 0.2)
                    End If

                    c.Left = CInt(fh * 0.7)
                    c.Width = ClientSize.Width - CInt(fh * 0.7 * 2)

                    If TypeOf c Is Label Then
                        If c.Name = "ExpandedInformation" AndAlso Form.blDetails.Text = "Show Details" Then
                            c.Visible = False
                            c.Height = 0
                        Else
                            c.Visible = True
                            Dim sz = g.MeasureString(c.Text, c.Font, c.Width)
                            c.Height = CInt(sz.Height + fh / 2)
                        End If
                    End If

                    If TryCast(c, CommandButton)?.AdjustSize() Then
                        LineBreaks += 1
                    End If

                    previous = c
                Next
            End Using
        End Sub
    End Class

    Class CommandButton
        Inherits ButtonEx

        Property Title As String
        Property Description As String

        Private _TitleFont As Font

        ReadOnly Property TitleFont As Font
            Get
                If _TitleFont Is Nothing Then
                    _TitleFont = New Font(Font.FontFamily, 11)
                End If

                Return _TitleFont
            End Get
        End Property

        Function AdjustSize() As Boolean
            Dim titleFontHeight = TitleFont.Height
            Dim h As Integer
            Dim hasLineBreak As Boolean

            If Title <> "" AndAlso Description <> "" Then
                Dim ts = GetTitleSize()
                Dim ds = GetDescriptionSize()
                h = CInt(titleFontHeight * 0.2 * 2) + ts.Height + ds.Height

                If titleFontHeight * 2 < ts.Height Then
                    hasLineBreak = True
                End If
            ElseIf Title <> "" Then
                Dim ts = GetTitleSize()
                h = CInt(titleFontHeight * 0.2 * 2) + ts.Height

                If titleFontHeight * 2 < ts.Height Then
                    hasLineBreak = True
                End If
            ElseIf Description <> "" Then
                Dim ds = GetDescriptionSize()
                h = CInt(titleFontHeight * 0.2 * 2) + ds.Height

                If FontHeight * 2 < ds.Height Then
                    hasLineBreak = True
                End If
            End If

            ClientSize = New Size(ClientSize.Width, h)
            Return hasLineBreak
        End Function

        Function GetTitleSize(Optional g1 As Graphics = Nothing) As Size
            If Title = "" Then
                Exit Function
            End If

            Dim g2 = g1

            If g2 Is Nothing Then
                g2 = CreateGraphics()
            End If

            Dim tf = TitleFont.Height
            Dim w = ClientSize.Width - CInt(tf * 0.3 * 2)
            Dim sz = g2.MeasureString(Title, TitleFont, w)

            If g1 Is Nothing Then
                g2.Dispose()
            End If

            Return New Size(CInt(sz.Width), CInt(sz.Height))
        End Function

        Function GetDescriptionSize(Optional g1 As Graphics = Nothing) As Size
            If Description = "" Then
                Exit Function
            End If

            Dim g2 = g1

            If g2 Is Nothing Then
                g2 = CreateGraphics()
            End If

            Dim tf = TitleFont.Height
            Dim w = ClientSize.Width - CInt(tf * 0.3 * 2)
            Dim sz = g2.MeasureString(Description, Font, w)

            If g1 Is Nothing Then
                g2.Dispose()
            End If

            Return New Size(CInt(sz.Width), CInt(sz.Height))
        End Function

        Protected Overrides Sub OnGotFocus(e As EventArgs)
            MyBase.OnGotFocus(e)
            BackColor = ThemeManager.CurrentTheme.General.Controls.Button.BackColor
            ForeColor = ThemeManager.CurrentTheme.General.Controls.Button.ForeColor
            FlatAppearance.BorderColor = ThemeManager.CurrentTheme.General.Controls.Button.BorderColor
        End Sub

        Protected Overrides Sub OnLostFocus(e As EventArgs)
            MyBase.OnLostFocus(e)
            BackColor = ThemeManager.CurrentTheme.ProcessingForm.ProcessButtonBackColor
            ForeColor = ThemeManager.CurrentTheme.ProcessingForm.ProcessButtonForeColor
            FlatAppearance.BorderColor = ThemeManager.CurrentTheme.General.BackColor
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)

            Dim g = e.Graphics
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            Dim titleFontHeight = TitleFont.Height

            Dim x = CInt(titleFontHeight * 0.3)
            Dim y = CInt(titleFontHeight * 0.2)
            Dim w = ClientSize.Width - x * 2
            Dim h = ClientSize.Height - CInt(titleFontHeight * 0.2 * 2)
            Dim r = New Rectangle(x, y, w, h)

            If Title <> "" AndAlso Description <> "" Then
                g.DrawString(Title, TitleFont, Brushes.White, r)
                y = CInt(titleFontHeight * 0.2) + GetTitleSize().Height
                r = New Rectangle(x, y, w, h)
                g.DrawString(Description, Font, Brushes.White, r)
            ElseIf Title <> "" Then
                g.DrawString(Title, TitleFont, Brushes.White, r)
            ElseIf Description <> "" Then
                g.DrawString(Description, Font, Brushes.White, r)
            End If
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            MyBase.Dispose(disposing)
            TitleFont.Dispose()
        End Sub
    End Class

    Sub blDetails_Click(sender As Object, e As EventArgs) Handles blDetails.Click
        paMain.ScrollControlIntoView(paMain.Controls(0))

        If blDetails.Text = "Show Details" Then
            blDetails.Text = "Hide Details"
        Else
            blDetails.Text = "Show Details"
        End If

        paMain.PerformLayout()
        AdjustHeight()
    End Sub

    Sub InputTextEditTextBoxKeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyData = Keys.Enter AndAlso Not AcceptButton Is Nothing Then
            AcceptButton.PerformClick()
        End If
    End Sub
End Class