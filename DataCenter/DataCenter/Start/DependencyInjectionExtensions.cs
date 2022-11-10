namespace Fibertest.DataCenter;

public static class DependencyInjectionExtensions
{
    // ������� ����� ���� ���������
    // � �������� ��������� ������������ �����������
    public static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<RtuRepo>(); // ��� ������� �������� �����
        services.AddSingleton<ClientCollection>(); 
        services.AddSingleton<RtuOccupations>(); 

        return services;
    }
}