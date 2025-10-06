using System.Runtime.CompilerServices;

// Позволяваме на тестовите проекти достъп до internal класове
[assembly: InternalsVisibleTo("OrderManagement.Tests.Unit")]
[assembly: InternalsVisibleTo("OrderManagement.Tests.Integration")]

// Ако използвате Moq за mocking на internal класове, добавете и това:
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]