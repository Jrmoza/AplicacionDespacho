﻿using System;
using System.Collections.Generic;
using System.Windows;
using AplicacionDespacho.Modules.Common.Base;
using AplicacionDespacho.Modules.Common.Models;
using AplicacionDespacho.Modules.Trazabilidad.Profiles.Testeador.Views;
using AplicacionDespacho.Modules.Trazabilidad.Profiles.Registrador.Views;

namespace AplicacionDespacho.Modules.Trazabilidad
{
    /// <summary>    
    /// Módulo de Trazabilidad con dos perfiles: Testeador y Registrador    
    /// </summary>    
    public class TrazabilidadModule : ModuleBase
    {
        private string _selectedProfile;

        public TrazabilidadModule(string profile = "Registrador")
        {
            _selectedProfile = profile;
        }

        protected override ModuleInfo CreateModuleInfo()
        {
            return new ModuleInfo
            {
                ModuleId = "Trazabilidad",
                DisplayName = "Trazabilidad",
                Description = "Gestión de trazabilidad de pallets y cajas",
                Icon = "📋",
                DisplayOrder = 2,
                IsEnabled = true,
                AvailableProfiles = new List<string> { "Testeador", "Registrador" }
            };
        }

        protected override ModulePermissions CreatePermissions()
        {
            return new ModulePermissions
            {
                CanReadPackingSJP = true,
                CanWritePackingSJP = true,  // CRÍTICO: Necesita eliminar pallets    
                CanReadDespachosSJP = true,
                CanWriteDespachosSJP = true,
                RequiresSuperUser = true
            };
        }

        protected override Window CreateMainWindow(string deviceId)
        {
            // Crear ventana según el perfil seleccionado    
            // NOTA: deviceId se ignora por ahora ya que las ventanas no lo necesitan en su constructor  
            switch (_selectedProfile)
            {
                case "Testeador":
                    return new TesteadorWindow();

                case "Registrador":
                    return new RegistradorWindow();

                default:
                    throw new ArgumentException($"Perfil desconocido: {_selectedProfile}");
            }
        }

        /// <summary>    
        /// Método adicional para cambiar de perfil sin reiniciar el módulo    
        /// </summary>    
        public void SwitchProfile(string newProfile)
        {
            if (!ModuleInfo.AvailableProfiles.Contains(newProfile))
            {
                throw new ArgumentException($"Perfil no válido: {newProfile}");
            }

            _selectedProfile = newProfile;
        }
    }
}