﻿Imports System.Data.SqlClient
Imports System.IO
Imports ZXing
Public Class frmInventoryFrozenMeat
    Private ds As DataSet = New DataSet()
    Private da As SqlDataAdapter
    Private intRowCount As Integer = 0I
    Private Sub frmInventoryFrozenMeat_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim intGroupBoxCount As Integer ' Number of GroupBoxes to create
        Dim intCols As Integer = 3 ' Number of columns
        Dim intGroupBoxWidth As Integer = 230
        Dim intGroupBoxHeight As Integer = 400
        Dim intGapX As Integer = 20 ' Gap between GroupBoxes in X direction
        Dim intGapY As Integer = 20 ' Gap between GroupBoxes in Y direction
        Dim strSql As String

        If OpenConnection() = True Then
            strSql = "Select * From Product Where product_category = 'Meat' And product_status = 'on shelves'"
            da = New SqlDataAdapter(strSql, connection)
            ds.Clear()
            da.Fill(ds, "Product")

            If ds.Tables("Product").Rows.Count > 0 Then
                intRowCount = ds.Tables("Product").Rows.Count
                intGroupBoxCount = ds.Tables("Product").Rows.Count
                For intI As Integer = 0 To intGroupBoxCount - 1
                    Dim intRow As Integer = intI \ intCols ' Calculate row index
                    Dim intCol As Integer = intI Mod intCols ' Calculate column index
                    Dim intPosX As Integer = 50 + intCol * (intGroupBoxWidth + intGapX)
                    Dim intPosY As Integer = 90 + intRow * (intGroupBoxHeight + intGapY)
                    Dim lblFrozenMeatName As New Label()
                    Dim lblFrozenMeatPrice As New Label()
                    Dim lblFrozenMeatQuantity As New Label()
                    Dim lblFrozenMeatDescription As New Label()
                    Dim grpFrozenMeat As New GroupBox()
                    Dim picFrozenMeat As New PictureBox()
                    Dim picFrozenMeatBarCode As New PictureBox()
                    Dim imageData As Byte() = Nothing
                    Dim dblPrice As Double
                    Dim writer As New BarcodeWriter
                    writer.Format = BarcodeFormat.CODE_128
                    With grpFrozenMeat
                        .Size = New Size(intGroupBoxWidth, intGroupBoxHeight)
                        .Location = New Point(intPosX, intPosY)
                        .BackColor = Color.White
                    End With
                    grpFrozenMeat.Tag = intI
                    AddHandler grpFrozenMeat.Click, AddressOf grpFrozenMeat_Click
                    imageData = ds.Tables("Product").Rows(intI).Item(5)

                    If imageData IsNot Nothing Then
                        ' Convert byte array to Image
                        Using memoryStream As New MemoryStream(imageData)
                            Dim imageFromDatabase As Image = Image.FromStream(memoryStream)

                            With picFrozenMeat
                                .Size = New Size(150, 140)
                                .Location = New Point(35, 40)
                                .Image = imageFromDatabase
                                .SizeMode = PictureBoxSizeMode.StretchImage
                                .BorderStyle = BorderStyle.FixedSingle
                            End With

                        End Using
                    Else
                        MessageBox.Show("No image found for the given condition.")
                    End If

                    With picFrozenMeatBarCode
                        .Size = New Size(150, 100)
                        .Location = New Point(35, 290)
                        .Image = writer.Write(ds.Tables("Product").Rows(intI).Item(7))
                        .SizeMode = PictureBoxSizeMode.StretchImage
                        .BorderStyle = BorderStyle.FixedSingle
                    End With
                    With lblFrozenMeatName
                        .Text = ds.Tables("Product").Rows(intI).Item(1)
                        .Location = New Point(65, 200)
                        .TextAlign = ContentAlignment.MiddleCenter
                    End With

                    If Not IsDBNull(ds.Tables("Product").Rows(intI).Item(10)) Then
                        If ds.Tables("Product").Rows(intI).Item(10).ToString() = "Active" Then
                            dblPrice = ds.Tables("Product").Rows(intI).Item(9)
                        ElseIf ds.Tables("Product").Rows(intI).Item(10).ToString() = "Pending" Then
                            dblPrice = ds.Tables("Product").Rows(intI).Item(2)
                        Else
                            dblPrice = ds.Tables("Product").Rows(intI).Item(2)
                        End If
                    Else
                        dblPrice = ds.Tables("Product").Rows(intI).Item(2)
                    End If
                    With lblFrozenMeatPrice
                        .Text = dblPrice.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("ms-MY")) & " " & "/ Pack"
                        .Location = New Point(70, 230)
                    End With

                    With lblFrozenMeatQuantity
                        .Text = "Quantity:" & " " & ds.Tables("Product").Rows(intI).Item(3)
                        .Location = New Point(80, 260)
                    End With

                    With lblFrozenMeatDescription
                        .Text = ds.Tables("Product").Rows(intI).Item(8)
                        .Location = New Point(35, 350) ' Adjust the location as needed
                        .AutoSize = False
                        .Size = New Size(150, 40) ' Set the size of the label to fit the description
                        .TextAlign = ContentAlignment.TopCenter
                        .ForeColor = Color.Black
                        .Visible = False
                    End With
                    grpFrozenMeat.Controls.Add(lblFrozenMeatName)
                    grpFrozenMeat.Controls.Add(lblFrozenMeatPrice)
                    grpFrozenMeat.Controls.Add(lblFrozenMeatQuantity)
                    grpFrozenMeat.Controls.Add(picFrozenMeat)
                    grpFrozenMeat.Controls.Add(picFrozenMeatBarCode)
                    grpFrozenMeat.Controls.Add(lblFrozenMeatDescription)
                    Me.Controls.Add(grpFrozenMeat)

                Next
            End If
            CloseConnection()
        Else
            MessageBox.Show("Error in connecting to database server", "Error", MessageBoxButtons.OK)
        End If
    End Sub

    Private Sub btnFrozenMeatCreate_Click(sender As Object, e As EventArgs) Handles btnFrozenMeatCreate.Click
        frmInventoryCreate.strCategory = "Meat"
        frmInventoryCreate.ShowDialog()
    End Sub

    Private Sub btnFrozenMeatEdit_Click(sender As Object, e As EventArgs) Handles btnFrozenMeatEdit.Click
        If intRowCount <= 0 Then
            MessageBox.Show("There have no item can be edit", "Alert!", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Else
            frmInventoryCreate.strCategory = "Meat"
            frmInventoryEdit.ShowDialog()
        End If
    End Sub
    Private Sub btnFrozenMeatRefresh_Click(sender As Object, e As EventArgs) Handles btnFrozenMeatRefresh.Click
        Dim groupBoxesToRemove As New List(Of GroupBox)
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is GroupBox Then
                groupBoxesToRemove.Add(DirectCast(ctrl, GroupBox))
            End If
        Next

        ' Remove all collected GroupBox controls
        For Each groupBox As GroupBox In groupBoxesToRemove
            Me.Controls.Remove(groupBox)
            groupBox.Dispose()
        Next
        frmInventoryFrozenMeat_Load(Nothing, Nothing)
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Dim searchText As String = txtSearch.Text.Trim().ToLower()
        Dim foundGroupBoxes As New List(Of GroupBox)

        ' Hide all GroupBoxes initially
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is GroupBox Then
                ctrl.Visible = False
            End If
        Next

        ' Search for matching items and store references
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is GroupBox Then
                Dim groupBox As GroupBox = DirectCast(ctrl, GroupBox)
                For Each innerCtrl As Control In groupBox.Controls
                    If TypeOf innerCtrl Is Label Then
                        Dim labelControl As Label = DirectCast(innerCtrl, Label)
                        If labelControl.Text.ToLower().Contains(searchText) Then
                            groupBox.Visible = True ' Show the GroupBox containing a match
                            foundGroupBoxes.Add(groupBox) ' Store the found GroupBox reference
                            Exit For
                        End If
                    End If
                Next
            End If
        Next

        ' Rearrange found GroupBoxes based on the layout format
        Dim intCols As Integer = 3 ' Number of columns
        Dim intGapX As Integer = 20 ' Gap between GroupBoxes in X direction
        Dim intGapY As Integer = 20 ' Gap between GroupBoxes in Y direction
        Dim intGroupBoxWidth As Integer = 230
        Dim intGroupBoxHeight As Integer = 400

        For i As Integer = 0 To foundGroupBoxes.Count - 1
            Dim intRow As Integer = i \ intCols ' Calculate row index
            Dim intCol As Integer = i Mod intCols ' Calculate column index
            Dim intPosX As Integer = 50 + intCol * (intGroupBoxWidth + intGapX)
            Dim intPosY As Integer = 90 + intRow * (intGroupBoxHeight + intGapY)
            foundGroupBoxes(i).Location = New Point(intPosX, intPosY)
        Next
    End Sub
End Class