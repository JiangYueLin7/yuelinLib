// See https://aka.ms/new-console-template for more information
using yuelinLib;

var vm = new ViewModel();
var view = new View();
view.Bind(vm);

// View => ViewMode
Console.WriteLine("Input Content:");

vm.Text = Console.ReadLine();

// ViewModel => View
vm.Text = "Hello, C#!";