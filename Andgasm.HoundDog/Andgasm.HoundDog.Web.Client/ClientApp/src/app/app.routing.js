"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var router_1 = require("@angular/router");
var home_component_1 = require("./home/home.component");
var dashboard_component_1 = require("./dashboard/dashboard.component");
var auth_guards_1 = require("./shared/guards/auth.guards");
var appRoutes = [
    { path: '', component: home_component_1.HomeComponent, pathMatch: 'full' },
    { path: 'dashboard', component: dashboard_component_1.DashboardComponent, pathMatch: 'full', canActivate: [auth_guards_1.AuthGuard] },
    { path: 'account', loadChildren: './account/account.module#AccountModule' },
    { path: 'navigation', loadChildren: './navigation/navigation.module#NavigationModule' },
];
exports.routing = router_1.RouterModule.forRoot(appRoutes);
