    var adminEmail = builder.Configuration["AdminSettings:AdminEmail"];
    var adminPassword = builder.Configuration["AdminSettings:AdminPassword"];

    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
    {
        app.Logger.LogWarning("AdminSettings:AdminEmail / AdminPassword tanımlı değil, yönetici hesabı oluşturulmadı.");
    }
    else
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, adminPassword);
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Yönetici"))
        {
            await userManager.AddToRoleAsync(adminUser, "Yönetici");
        }
    }
    
}

app.Run();
