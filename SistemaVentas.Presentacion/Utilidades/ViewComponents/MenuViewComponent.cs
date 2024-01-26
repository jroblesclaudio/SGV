﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaVentas.Presentacion.Models.ViewModels;
using SistemaVentas.BLL.Interfaces;

namespace SistemaVentas.Presentacion.Utilidades.ViewComponents
{
    public class MenuViewComponent:ViewComponent
    {
        private readonly IMenuService _menuService;
        private readonly IMapper _mapper;

        public MenuViewComponent(IMenuService menuService, IMapper mapper)
        {
            _menuService = menuService;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            List<VMMenu> listaMenus;

            if(claimUser.Identity.IsAuthenticated)
            {
                string idUsuario = claimUser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault();

                listaMenus = _mapper.Map<List<VMMenu>>(await _menuService.ObtenerMenus(int.Parse(idUsuario)));
            }
            else
            {
                listaMenus=new List<VMMenu> { };
            }
            
            return View(listaMenus);
        }
    }
}
