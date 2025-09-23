<%@ Page Title="Crear cuenta" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="GymWebForms.Account.Register" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" rel="stylesheet" />

<div class="row justify-content-center mt-5">
  <div class="col-12 col-md-6 col-lg-5">
    <div class="card shadow-sm border-0 rounded-4">
      <div class="card-body p-4">
        <div class="text-center mb-3">
          <i class="bi bi-person-plus-fill" style="font-size:2.2rem;"></i>
          <h4 class="mt-2 mb-0">Crear cuenta</h4>
          <small class="text-muted">Únete para gestionar tus rutinas y progreso</small>
        </div>

        <asp:ValidationSummary runat="server" CssClass="alert alert-danger" />

        <div class="mb-3">
          <label class="form-label">Correo electrónico</label>
          <div class="input-group">
            <span class="input-group-text"><i class="bi bi-envelope"></i></span>
            <asp:TextBox runat="server" ID="txtEmail" CssClass="form-control" placeholder="tucorreo@dominio.com" />
          </div>
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" CssClass="text-danger" ErrorMessage="El correo es obligatorio." />
          <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" CssClass="text-danger"
            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="Formato de correo inválido." />
        </div>

        <div class="mb-3">
          <label class="form-label">Contraseña</label>
          <div class="input-group">
            <span class="input-group-text"><i class="bi bi-lock"></i></span>
            <asp:TextBox runat="server" ID="txtPassword" TextMode="Password" CssClass="form-control" placeholder="Mínimo 8 caracteres" />
          </div>
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" CssClass="text-danger" ErrorMessage="La contraseña es obligatoria." />
          <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPassword" CssClass="text-danger"
            ValidationExpression="^(?=.{8,}).*$" ErrorMessage="Mínimo 8 caracteres." />
        </div>

        <div class="mb-4">
          <label class="form-label">Confirmar contraseña</label>
          <div class="input-group">
            <span class="input-group-text"><i class="bi bi-shield-check"></i></span>
            <asp:TextBox runat="server" ID="txtConfirm" TextMode="Password" CssClass="form-control" placeholder="Repite tu contraseña" />
          </div>
          <asp:CompareValidator runat="server" ControlToValidate="txtConfirm" ControlToCompare="txtPassword"
            CssClass="text-danger" ErrorMessage="Las contraseñas no coinciden." />
        </div>

        <asp:Button runat="server" ID="btnRegister" Text="Registrarme" CssClass="btn btn-primary w-100"
                    OnClick="btnRegister_Click" />
        <div class="text-center mt-3">
          <a href="~/Account/Login.aspx" class="link-secondary text-decoration-none">
            <i class="bi bi-box-arrow-in-right"></i> Ya tengo cuenta
          </a>
        </div>
      </div>
    </div>
  </div>
</div>
</asp:Content>
