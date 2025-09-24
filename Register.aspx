<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="PEAKGYMM.Register" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
  <div class="row justify-content-center mt-5">
    <div class="col-12 col-md-6 col-lg-5">
      <div class="card shadow-sm border-0 rounded-4">
        <div class="card-body p-4">
          <h4 class="mb-3 text-center">Crear cuenta</h4>

          <asp:ValidationSummary runat="server" CssClass="alert alert-danger" />

          <div class="mb-3">
            <label class="form-label">Correo electrónico</label>
            <asp:TextBox runat="server" ID="txtEmail" CssClass="form-control" placeholder="tucorreo@dominio.com" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" CssClass="text-danger" ErrorMessage="El correo es obligatorio." />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" CssClass="text-danger"
              ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="Correo inválido." />
          </div>

          <div class="mb-3">
            <label class="form-label">Contraseña</label>
            <asp:TextBox runat="server" ID="txtPassword" TextMode="Password" CssClass="form-control" placeholder="Mínimo 8 caracteres" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" CssClass="text-danger" ErrorMessage="La contraseña es obligatoria." />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPassword" CssClass="text-danger"
              ValidationExpression="^(?=.{8,}).*$" ErrorMessage="Mínimo 8 caracteres." />
          </div>

          <div class="mb-4">
            <label class="form-label">Confirmar contraseña</label>
            <asp:TextBox runat="server" ID="txtConfirm" TextMode="Password" CssClass="form-control" placeholder="Repite tu contraseña" />
            <asp:CompareValidator runat="server" ControlToValidate="txtConfirm" ControlToCompare="txtPassword"
              CssClass="text-danger" ErrorMessage="Las contraseñas no coinciden." />
          </div>

          <asp:Button runat="server" ID="btnRegister" Text="Registrarme" CssClass="btn btn-primary w-100"
                      OnClick="btnRegister_Click" />
        </div>
      </div>
    </div>
  </div>
</asp:Content>
