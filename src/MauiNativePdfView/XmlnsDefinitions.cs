using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

// Define custom XAML namespace schema for MauiNativePdfView
// This allows users to use: xmlns:pdf="http://eightbot.com/maui/pdfview"
// instead of: xmlns:pdf="clr-namespace:MauiNativePdfView;assembly=MauiNativePdfView"

[assembly: XmlnsDefinition("http://eightbot.com/maui/pdfview", "MauiNativePdfView")]
[assembly: XmlnsDefinition("http://eightbot.com/maui/pdfview", "MauiNativePdfView.Abstractions")]

// Suggest "pdf" as the default prefix when users add this namespace
[assembly: XmlnsPrefix("http://eightbot.com/maui/pdfview", "pdf")]

// Ensure types are preserved during linking
[assembly: Preserve(AllMembers = true)]
