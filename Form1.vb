
Imports System.Net.Http
Imports System.Xml.Linq
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class Form1

  Public Async Function ApiAufrufenAsync(url As String) As Task(Of String)

    Try
      Using client As New HttpClient()

        client.Timeout = TimeSpan.FromSeconds(30)

        Dim response As HttpResponseMessage =
            Await client.GetAsync(url)

        ' HTTP-Fehler wie 404, 500 usw. erkennen
        response.EnsureSuccessStatusCode()

        ' JSON als String zurückgeben
        Dim json As String =
            Await response.Content.ReadAsStringAsync()

        Return json

      End Using

    Catch ex As TaskCanceledException
      ' Timeout
      Return "{""error"":""TaskCanceledException""}"

    Catch ex As HttpRequestException
      ' HTTP- oder Netzwerkfehler
      Return "{""error"":""HttpRequestException""}"

    Catch ex As Exception
      ' Sonstige Fehler
      Return "{""error"":""Exception""}"

    End Try

  End Function

  Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
    Dim url As String = "http://www.efa-bw.de/nvbw/XML_DM_REQUEST?typeInfo_dm=stopID&nameInfo_dm=6900090&deleteAssignedStops_dm=0&mode=direct&useRealtime=1&limit=10"

    Dim json As String = Await ApiAufrufenAsync(url)

    LoadXmlToTreeView(json)


  End Sub


  Public Sub LoadXmlToTreeView(pXml As String)
    Try
      ' TreeView leeren und Performance-Optimierung aktivieren
      TreeView1.Nodes.Clear()
      TreeView1.BeginUpdate()

      ' XML hocheffizient einlesen
      Dim doc As XDocument = XDocument.Parse(pXml)

      ' Das Wurzelelement ermitteln
      Dim rootElement As XElement = doc.Root

      If rootElement IsNot Nothing Then
        ' Rekursiven Aufbau starten
        Dim rootNode As New TreeNode(rootElement.Name.LocalName)
        TreeView1.Nodes.Add(rootNode)

        ' Attribute und Kindelemente hinzufügen
        AddAttributes(rootElement, rootNode)
        ParseXmlElement(rootElement, rootNode)

        ' Baum standardmäßig expandieren
        rootNode.Expand()
      End If

    Catch ex As Exception
      MessageBox.Show($"Fehler beim Laden der XML: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
    Finally
      ' TreeView-Zeichnung wieder freigeben
      TreeView1.EndUpdate()
    End Try
  End Sub

  ''' <summary>
  ''' Rekursives Durchlaufen aller XML-Elemente.
  ''' </summary>
  Private Sub ParseXmlElement(element As XElement, parentNode As TreeNode)
    For Each childElement As XElement In element.Elements()
      ' Knotentext standardmäßig auf den Elementnamen setzen
      Dim nodeText As String = childElement.Name.LocalName

      ' Wenn das Element keine weiteren Kinder hat, aber Text enthält, diesen anhängen
      If Not childElement.HasElements And Not String.IsNullOrWhiteSpace(childElement.Value) Then
        nodeText &= $": {childElement.Value.Trim()}"
      End If

      Dim childNode As New TreeNode(nodeText)
      parentNode.Nodes.Add(childNode)

      ' Attribute des aktuellen Elements auslesen
      AddAttributes(childElement, childNode)

      ' Tiefer in die Hierarchie springen (Rekursion)
      ParseXmlElement(childElement, childNode)
    Next
  End Sub

  ''' <summary>
  ''' Fügt dem TreeNode vorhandene XML-Attribute als Unterknoten hinzu.
  ''' </summary>
  Private Sub AddAttributes(element As XElement, node As TreeNode)
    If element.HasAttributes Then
      For Each attr As XAttribute In element.Attributes()
        ' Attribute visuell kennzeichnen (z.B. mit einem @-Symbol)
        Dim attrNode As New TreeNode($"@{attr.Name.LocalName} = ""{attr.Value}""")
        ' Optional: Farbe für Attribute anpassen, um sie von Elementen zu unterscheiden
        attrNode.ForeColor = Color.DarkSlateGray
        node.Nodes.Add(attrNode)
      Next
    End If
  End Sub




End Class
