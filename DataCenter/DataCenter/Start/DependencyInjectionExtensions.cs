namespace Fibertest.DataCenter.Start;

public static class DependencyInjectionExtensions
{
    // ������� ����� ���� ���������
    // � �������� ��������� ������������ �����������
    public static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<RtuRepo>(); // ��� ������� �������� �����
        // services.AddScoped<class2>();
        // services.AddScoped<class3>();

        return services;
    }
}