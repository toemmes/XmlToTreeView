
Imports System.Net.Http
Imports System.Net.Http.Headers

''' <summary>
''' Die Hauptklasse der Windows Forms-Anwendung, die Funktionen zum Abrufen von XML-Daten von einer API und zum Anzeigen dieser Daten in einem TreeView-Steuerelement bereitstellt.
''' </summary>
Public Class Form1

  '' HttpClient als private Instanz für die gesamte Form
  Private ReadOnly _httpClient As New HttpClient()

  ''' <summary>
  ''' Asynchrone Methode zum Abrufen von XML-Daten von einer angegebenen URL.
  ''' </summary>
  ''' <param name="url">Die URL der XML-Daten</param>
  ''' <returns>Die XML-Daten als String</returns>
  Public Async Function GetXmlAsync(url As String) As Task(Of String)

    Try
      ' Prüfen, ob eine URL übergeben wurde
      If String.IsNullOrWhiteSpace(url) Then
        Throw New ArgumentException("Die URL darf nicht leer sein.")
      End If

      ' XML als Antwort anfordern
      Using request As New HttpRequestMessage(HttpMethod.Get, url)

        request.Headers.Accept.Clear()
        request.Headers.Accept.Add(
                    New MediaTypeWithQualityHeaderValue("application/xml")
                )

        ' HTTP Request ausführen
        Using response As HttpResponseMessage =
                    Await _httpClient.SendAsync(request)

          ' XML-Inhalt lesen
          Dim xml As String =
                        Await response.Content.ReadAsStringAsync()

          ' HTTP-Fehler wie 404 oder 500
          If Not response.IsSuccessStatusCode Then

            Return "<error></error>"

          End If

          Return xml

        End Using

      End Using

    Catch ex As TaskCanceledException

      Return "<error>" &
                   "<message>Timeout beim HTTP Request</message>" &
                   "</error>"
    Catch ex As HttpRequestException

      Return "<error>" &
             "<message>" & XmlEscape(ex.Message) & "</message>" &
             "</error>"

    Catch ex As Exception

      Return "<error>" &
             "<message>" & XmlEscape(ex.Message) & "</message>" &
             "</error>"
    End Try

  End Function

  ''' <summary>
  ''' Escaped einen String für die Verwendung in XML.
  ''' </summary>
  ''' <param name="value">Der zu escapende String</param>
  ''' <returns>Der escapede String</returns>
  Private Function XmlEscape(value As String) As String

    If value Is Nothing Then Return ""

    Return System.Security.SecurityElement.Escape(value)

  End Function

  ''' <summary>
  ''' Asynchrone Methode zum Abrufen von XML-Daten von einer angegebenen URL mit einem optionalen Timeout.
  ''' </summary>
  ''' <param name="url"></param>
  ''' <param name="timeoutSeconds"></param>
  ''' <returns></returns>
  Public Async Function ApiAufrufenAsync(url As String, Optional timeoutSeconds As Integer = 30) As Task(Of String)
    ' Prüfen, ob die URL null oder leer ist
    If String.IsNullOrWhiteSpace(url) Then
      Throw New ArgumentException("Die URL darf nicht leer sein.", NameOf(url))
    End If
    ' Timeout für die Anfrage festlegen
    Using cts As New Threading.CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds))
      ' 
      Using request As New HttpRequestMessage(HttpMethod.Get, url)
        request.Headers.Accept.Clear()
        request.Headers.Accept.Add(New MediaTypeWithQualityHeaderValue("application/xml"))
        ' HTTP-Request ausführen
        Dim response As HttpResponseMessage = Await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(False)
        ' HTTP-Fehler wie 404, 500 usw. erkennen
        response.EnsureSuccessStatusCode()
        ' XML als String zurückgeben
        Dim content As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
        Return content
      End Using
    End Using
  End Function
  'Public Async Function ApiAufrufenAsync(url As String) As Task(Of String)

  '  Try
  '    ' Prüfen, ob eine URL übergeben wurde
  '    Using client As New HttpClient()
  '      ' Timeout auf 30 Sekunden setzen
  '      client.Timeout = TimeSpan.FromSeconds(30)
  '      ' HTTP-Request ausführen
  '      Dim response As HttpResponseMessage =
  '          Await client.GetAsync(url)

  '      ' HTTP-Fehler wie 404, 500 usw. erkennen
  '      response.EnsureSuccessStatusCode()

  '      ' JSON als String zurückgeben
  '      Dim json As String =
  '          Await response.Content.ReadAsStringAsync()


  '      Return json

  '    End Using

  '  Catch ex As TaskCanceledException
  '    ' Timeout
  '    Return "{""error"":""TaskCanceledException""}"

  '  Catch ex As HttpRequestException
  '    ' HTTP- oder Netzwerkfehler
  '    Return "{""error"":""HttpRequestException""}"

  '  Catch ex As Exception
  '    ' Sonstige Fehler
  '    Return "{""error"":""Exception""}"

  '  End Try

  'End Function

  ''' <summary>
  ''' Event-Handler für den Klick auf Button1. Ruft die API auf und lädt die XML-Daten in das TreeView.
  ''' </summary>
  ''' <param name="sender">Das auslösende Steuerelement</param>
  ''' <param name="e">Die Ereignisdaten</param>
  Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

    Try
      Button1.Enabled = False
      Cursor = Cursors.WaitCursor

      TreeView1.Nodes.Clear()
      Dim url As String = "https://www.efa-bw.de/nvbw/XML_DM_REQUEST?typeInfo_dm=stopID&nameInfo_dm=6900090&deleteAssignedStops_dm=0&mode=direct&useRealtime=1&limit=10"
      Dim xml As String = Await ApiAufrufenAsync(url) ' einheitlich XML erwarten
      LoadXmlToTreeView(xml)
    Catch ex As Exception
      MessageBox.Show($"Fehler beim Abrufen/Parsen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
      ' optional: Logging ins Logfile
    Finally
      Button1.Enabled = True
      Cursor = Cursors.Default
    End Try

    'TreeView1.Nodes.Clear()
    '' URL der API, die XML-Daten zurückgibt
    'Dim url As String = "http://www.efa-bw.de/nvbw/XML_DM_REQUEST?typeInfo_dm=stopID&nameInfo_dm=6900090&deleteAssignedStops_dm=0&mode=direct&useRealtime=1&limit=10"
    '' Asynchrone Methode aufrufen, um die API zu erreichen und die XML-Daten abzurufen
    'Dim json As String = Await ApiAufrufenAsync(url)
    ''
    'LoadXmlToTreeView(json)


  End Sub

#Region "XML in TreeView laden"

  ''' <summary>
  ''' Lädt XML-Daten in das TreeView-Steuerelement.
  ''' </summary>
  ''' <param name="pXml">Die XML-Daten als String</param>
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
  ''' Rekursive Methode zum Parsen von XML-Elementen und Hinzufügen zu TreeNodes.
  ''' </summary>
  ''' <param name="element">Das zu parsende XML-Element</param>
  ''' <param name="parentNode">Der übergeordnete TreeNodes</param>
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
  ''' Fügt die Attribute eines XML-Elements als TreeNodes hinzu.  
  ''' </summary>
  ''' <param name="element">Das XML-Element, dessen Attribute hinzugefügt werden</param>
  ''' <param name="node">Der TreeNodes, dem die Attribute hinzugefügt werden</param>
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

#End Region

  ''' <summary>
  ''' Event-Handler für den Klick auf Button2. Ruft die XML-Daten von der API ab und lädt sie in das TreeView.
  ''' </summary>
  ''' <param name="sender">Das auslösende Steuerelement</param>
  ''' <param name="e">Die Ereignisdaten</param>
  Private Async Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

    TreeView1.Nodes.Clear()

    Dim url As String = "http://www.efa-bw.de/nvbw/XML_DM_REQUEST?typeInfo_dm=stopID&nameInfo_dm=6900090&deleteAssignedStops_dm=0&mode=direct&useRealtime=1&limit=10"

    Dim json As String = Await GetXmlAsync(url)

    LoadXmlToTreeView(json)
  End Sub
End Class
