<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="PEAKGYMM.Login" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
  <style>
    :root{ --brand-purple:#6f42c1; --brand-black:#000; --brand-white:#f8f9fa; }
    .auth-wrap{ min-height: calc(100vh - 140px); display:grid; place-items:center;
      background: linear-gradient(180deg, var(--brand-black), #0a0a0a); color: var(--brand-white); }
    .auth-card{ background: rgba(255,255,255,.03); border:1px solid rgba(255,255,255,.08);
      border-radius:1.25rem; padding:2rem; box-shadow:0 10px 30px rgba(0,0,0,.35);
      backdrop-filter: blur(4px); max-width: 520px; width:100%; }
    .btn-cta{ --bs-btn-color:#fff; --bs-btn-bg:var(--brand-purple); --bs-btn-border-color:var(--brand-purple);
      --bs-btn-hover-bg:#5a35a0; --bs-btn-hover-border-color:#5a35a0; box-shadow:0 8px 24px rgba(111,66,193,.35); }
    .text-soft{ color:#cfcfd6; }
  </style>

  <section class="auth-wrap">
    <div class="auth-card">
      <div class="text-center mb-3">
        <i class="bi bi-box-arrow-in-right" style="font-size:2rem;color:var(--brand-purple)"></i>
        <h3 class="mt-2 mb-0">Iniciar sesión</h3>
        <small class="text-soft">Accede a tu panel</small>
      </div>

      <!-- avisos -->
      <asp:Panel runat="server" ID="pnlRegistered" Visible="false" CssClass="alert alert-success">
        Cuenta creada. Ahora inicia sesión.
      </asp:Panel>
      <asp:Panel runat="server" ID="pnlError" Visible="false" CssClass="alert alert-danger"></asp:Panel>

      <div class="mb-3">
        <label class="form-label">Correo electrónico</label>
        <div class="input-group">
          <span class="input-group-text"><i class="bi bi-envelope"></i></span>
          <asp:TextBox runat="server" ID="txtEmail" CssClass="form-control" placeholder="tucorreo@dominio.com" />
        </div>
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail"
          CssClass="text-danger" ErrorMessage="Ingresa tu correo." />
      </div>

      <div class="mb-3">
        <label class="form-label">Contraseña</label>
        <div class="input-group">
          <span class="input-group-text"><i class="bi bi-lock"></i></span>
          <asp:TextBox runat="server" ID="txtPassword" TextMode="Password" CssClass="form-control" placeholder="Tu contraseña" />
        </div>
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword"
          CssClass="text-danger" ErrorMessage="Ingresa tu contraseña." />
      </div>

      <div class="d-flex justify-content-between align-items-center mb-3">
        <div class="form-check">
          <asp:CheckBox runat="server" ID="chkRemember" CssClass="form-check-input" />
          <label class="form-check-label" for="chkRemember">Recordarme</label>
        </div>
        <a href="Register.aspx" class="link-light text-decoration-none">
          <i class="bi bi-person-plus"></i> Registrarme
        </a>
      </div>

      <asp:Button runat="server" ID="btnLogin" Text="Entrar" CssClass="btn btn-cta w-100"
        OnClick="btnLogin_Click" />
    </div>
  </section>
</asp:Content>
