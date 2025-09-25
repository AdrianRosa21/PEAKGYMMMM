<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="PEAKGYMM.Register" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
  <!-- Bootstrap Icons (si ya está en Site.Master, quita esta línea) -->
  <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" rel="stylesheet" />

  <style>
    :root{
      --brand-purple:#6f42c1;
      --brand-purple-hover:#5a35a0;
      --panel-bg:#0f0f13;
      --panel-border:rgba(255,255,255,.08);
    }
    .card-neo{
      background:
        radial-gradient(1200px 600px at 10% 10%, rgba(111,66,193,.12), transparent 60%),
        radial-gradient(1000px 500px at 90% 20%, rgba(111,66,193,.10), transparent 60%),
        linear-gradient(180deg, #0b0b0d, #0e0e13);
      color:#fff; border:1px solid var(--panel-border);
      border-radius:1.25rem; box-shadow:0 12px 36px rgba(0,0,0,.45);
      backdrop-filter: blur(4px);
    }
    .btn-brand{
      --bs-btn-color:#fff;
      --bs-btn-bg:var(--brand-purple);
      --bs-btn-border-color:var(--brand-purple);
      --bs-btn-hover-bg:var(--brand-purple-hover);
      --bs-btn-hover-border-color:var(--brand-purple-hover);
      --bs-btn-focus-shadow-rgb:111,66,193;
      box-shadow:0 10px 28px rgba(111,66,193,.35);
    }
    .form-control, .form-select{
      background:#0f0f13; color:#e8e8f0; border:1px solid rgba(255,255,255,.14);
      transition:border-color .15s, box-shadow .15s;
    }
    .form-control:focus, .form-select:focus{
      border-color: rgba(111,66,193,.85);
      box-shadow:0 0 0 .25rem rgba(111,66,193,.25);
      color:#fff;
    }
    .input-group>.btn{
      border-color: rgba(255,255,255,.14);
      background:#141419; color:#cfcfe3;
    }

    /* ===== Overrides de focus: texto negro + fondo blanco ===== */
  .card-neo .form-control:focus,
  .card-neo .form-select:focus {
    background:#fff !important;
    color:#000 !important;
    border-color:#6f42c1 !important;
    box-shadow:0 0 0 .25rem rgba(111,66,193,.25) !important;
  }

  /* Placeholders visibles sobre blanco */
  .card-neo .form-control::placeholder { color:#6c757d; opacity:1; }

  /* Select: al abrir/focus, opciones negras en fondo blanco */
  .card-neo .form-select:focus,
  .card-neo .form-select:focus option {
    background:#fff !important;
    color:#000 !important;
  }

  /* Date input: icono bien sobre fondo claro (Chrome/WebKit) */
  .card-neo input[type="date"]:focus::-webkit-calendar-picker-indicator { filter:none; }

  /* Autofill (Chrome): forzar texto negro y fondo blanco */
  .card-neo .form-control:-webkit-autofill {
    -webkit-text-fill-color:#000 !important;
    box-shadow:0 0 0 1000px #fff inset !important;
    transition: background-color 5000s ease-in-out 0s; /* evita parpadeo */
  }

  /* Botón del ojo: que contraste cuando el input está en blanco */
  .card-neo .input-group > .btn {
    background:#f2f2f2;
    color:#333;
    border-color:#ddd;
  }
  </style>

  <script>
      function togglePassword(inputId, iconId) {
          const input = document.getElementById(inputId);
          const icon = document.getElementById(iconId);
          if (!input) return;
          if (input.type === "password") {
              input.type = "text";
              icon.classList.replace("bi-eye", "bi-eye-slash");
          } else {
              input.type = "password";
              icon.classList.replace("bi-eye-slash", "bi-eye");
          }
      }

      // Validación cliente: fecha no futuro y +18
      function validateBirthDate(sender, args) {
          const v = args.Value; // yyyy-MM-dd
          if (!v) { args.IsValid = false; return; }
          const d = new Date(v + "T00:00:00");
          const today = new Date();
          if (d > today) { args.IsValid = false; return; }
          // >= 18 años
          const d18 = new Date(d); d18.setFullYear(d18.getFullYear() + 18);
          args.IsValid = d18 <= today;
      }
  </script>

  <div class="row justify-content-center mt-5">
    <div class="col-12 col-md-7 col-lg-6">
      <div class="card card-neo border-0">
        <div class="card-body p-4 p-md-5">

          <h3 class="mb-4 text-center fw-bold">
            <i class="bi bi-person-plus me-2"></i>Crear cuenta
          </h3>

          <asp:ValidationSummary runat="server" CssClass="alert alert-danger"
                                 ValidationGroup="reg" />

          <!-- Email -->
          <div class="mb-3">
            <label class="form-label">Correo electrónico</label>
            <asp:TextBox runat="server" ID="txtEmail" CssClass="form-control"
                         placeholder="tucorreo@dominio.com" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail"
                CssClass="text-danger" ErrorMessage="El correo es obligatorio."
                ValidationGroup="reg" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail"
                CssClass="text-danger" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                ErrorMessage="Correo inválido." ValidationGroup="reg" />
          </div>

          <!-- Contraseña -->
          <div class="mb-3">
            <label class="form-label">Contraseña</label>
            <div class="input-group">
              <asp:TextBox runat="server" ID="txtPassword" TextMode="Password"
                           CssClass="form-control" placeholder="Mínimo 8 caracteres"
                           ClientIDMode="Static" />
              <button type="button" class="btn" onclick="togglePassword('txtPassword','toggleIcon1')">
                <i id="toggleIcon1" class="bi bi-eye"></i>
              </button>
            </div>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword"
                CssClass="text-danger" ErrorMessage="La contraseña es obligatoria."
                ValidationGroup="reg" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPassword"
                CssClass="text-danger" ValidationExpression="^(?=.{8,}).*$"
                ErrorMessage="Mínimo 8 caracteres." ValidationGroup="reg" />
          </div>

          <!-- Confirmar -->
          <div class="mb-3">
            <label class="form-label">Confirmar contraseña</label>
            <div class="input-group">
              <asp:TextBox runat="server" ID="txtConfirm" TextMode="Password"
                           CssClass="form-control" placeholder="Repite tu contraseña"
                           ClientIDMode="Static" />
              <button type="button" class="btn" onclick="togglePassword('txtConfirm','toggleIcon2')">
                <i id="toggleIcon2" class="bi bi-eye"></i>
              </button>
            </div>
            <asp:CompareValidator runat="server" ControlToValidate="txtConfirm"
                ControlToCompare="txtPassword" CssClass="text-danger"
                ErrorMessage="Las contraseñas no coinciden." ValidationGroup="reg" />
          </div>

          <!-- Nombre -->
          <div class="mb-3">
            <label class="form-label">Nombre completo</label>
            <asp:TextBox runat="server" ID="txtFullName" CssClass="form-control" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName"
                CssClass="text-danger" ErrorMessage="Nombre requerido."
                ValidationGroup="reg" />
          </div>

          <!-- Nacimiento -->
          <div class="mb-3">
            <label class="form-label">Fecha de nacimiento</label>
            <asp:TextBox runat="server" ID="txtBirthDate" TextMode="Date" CssClass="form-control" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtBirthDate"
                CssClass="text-danger" ErrorMessage="La fecha de nacimiento es obligatoria."
                ValidationGroup="reg" />
            <asp:CustomValidator runat="server" ID="cvBirth"
                ControlToValidate="txtBirthDate" ClientValidationFunction="validateBirthDate"
                CssClass="text-danger" ErrorMessage="Debes ser mayor de 18 y no puedes elegir una fecha futura."
                ValidateEmptyText="true" ValidationGroup="reg" />
          </div>

          <!-- Sexo -->
          <div class="mb-3">
            <label class="form-label">Sexo</label>
            <asp:DropDownList runat="server" ID="ddlGender" CssClass="form-select">
              <asp:ListItem Text="Seleccionar..." Value="" />
              <asp:ListItem Text="Masculino" Value="M" />
              <asp:ListItem Text="Femenino" Value="F" />
              <asp:ListItem Text="Otro" Value="O" />
            </asp:DropDownList>
          </div>

          <!-- Peso -->
          <div class="mb-3">
            <label class="form-label">Peso (kg)</label>
            <asp:TextBox runat="server" ID="txtWeight" CssClass="form-control" placeholder="Ej. 72.5" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtWeight"
                CssClass="text-danger" ValidationExpression="^\d{1,3}([.,]\d{1,2})?$"
                ErrorMessage="Peso inválido (usa 0-300, opcional 2 decimales)."
                ValidationGroup="reg" />
          </div>

          <!-- Altura -->
          <div class="mb-3">
            <label class="form-label">Altura (cm)</label>
            <asp:TextBox runat="server" ID="txtHeight" CssClass="form-control" placeholder="Ej. 176" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtHeight"
                CssClass="text-danger" ValidationExpression="^\d{2,3}([.,]\d{1,2})?$"
                ErrorMessage="Altura inválida (usa 50-250 cm)."
                ValidationGroup="reg" />
          </div>

          <!-- Teléfono -->
          <div class="mb-3">
            <label class="form-label">Teléfono</label>
            <asp:TextBox runat="server" ID="txtPhone" CssClass="form-control" placeholder="+503 7000-0000" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPhone"
                CssClass="text-danger" ValidationExpression="^[\d+\-\s()]{7,20}$"
                ErrorMessage="Teléfono inválido." ValidationGroup="reg" />
          </div>

          <!-- Foto -->
          <div class="mb-4">
            <label class="form-label">Foto de perfil</label>
            <asp:FileUpload runat="server" ID="fuProfileImage" CssClass="form-control" />
          </div>

          <asp:Button runat="server" ID="btnRegister" Text="Registrarme"
                      CssClass="btn btn-brand w-100" OnClick="btnRegister_Click"
                      ValidationGroup="reg" />
        </div>
      </div>
    </div>
  </div>
</asp:Content>
