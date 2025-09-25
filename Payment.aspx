<%@ Page Title="Membresías" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Payment.aspx.cs" Inherits="PEAKGYMM.Payment" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

  

  <!-- ===== Estilos de marca (morado) ===== -->
  <style>
    :root{
      --brand-purple:#6f42c1;
      --brand-purple-hover:#5a35a0;
    }
    .btn-brand{
      --bs-btn-color:#fff;
      --bs-btn-bg:var(--brand-purple);
      --bs-btn-border-color:var(--brand-purple);
      --bs-btn-hover-bg:var(--brand-purple-hover);
      --bs-btn-hover-border-color:var(--brand-purple-hover);
      --bs-btn-focus-shadow-rgb:111,66,193;
      box-shadow:0 8px 24px rgba(111,66,193,.35);
    }
    .plan-card{ background:#0f0f13; color:#fff; border:1px solid rgba(255,255,255,.08); transition:.2s }
    .plan-card:hover{ transform:translateY(-4px); box-shadow:0 12px 32px rgba(0,0,0,.45) }
  </style>

  <!-- ===== Campos ocultos que viajan al servidor ===== -->
  <asp:HiddenField runat="server" ID="hidPlan" />
  <asp:HiddenField runat="server" ID="hidPrice" />

  <div class="container my-5">
    <div class="text-center mb-4">
      <h2 class="fw-bold">Elige tu plan</h2>
      <p class="text-muted">Activa tu membresía y paga en un solo paso</p>
    </div>

    <asp:ValidationSummary runat="server" CssClass="alert alert-danger"
                       ValidationGroup="pay" />


    <div class="row g-4">
      <!-- Mensual -->
      <div class="col-12 col-md-4">
        <div class="card h-100 shadow-lg border-0 rounded-4 plan-card">
          <div class="card-body p-4">
            <div class="d-flex align-items-center mb-2">
              <i class="bi bi-lightning-charge fs-3 me-2"></i>
              <h4 class="mb-0">Mensual</h4>
            </div>
            <h2 class="fw-bold mb-3">$15</h2>
            <ul class="list-unstyled small">
              <li><i class="bi bi-check2-circle me-1"></i>Acceso total 30 días</li>
              <li><i class="bi bi-check2-circle me-1"></i>Rutinas base</li>
              <li><i class="bi bi-check2-circle me-1"></i>Seguimiento de progreso</li>
            </ul>
            <button type="button" class="btn btn-brand w-100 select-plan" data-plan="Mensual">
              Elegir Mensual
            </button>
          </div>
        </div>
      </div>

      <!-- Trimestral -->
      <div class="col-12 col-md-4">
        <div class="card h-100 shadow-lg border-0 rounded-4 plan-card">
          <div class="card-body p-4">
            <div class="d-flex align-items-center mb-2">
              <i class="bi bi-stars fs-3 me-2"></i>
              <h4 class="mb-0">Trimestral</h4>
            </div>
            <h2 class="fw-bold mb-3">$40</h2>
            <ul class="list-unstyled small">
              <li><i class="bi bi-check2-circle me-1"></i>Acceso 3 meses</li>
              <li><i class="bi bi-check2-circle me-1"></i>Plan personalizado</li>
              <li><i class="bi bi-check2-circle me-1"></i>Métricas avanzadas</li>
            </ul>
            <button type="button" class="btn btn-brand w-100 select-plan" data-plan="Trimestral">
              Elegir Trimestral
            </button>
          </div>
        </div>
      </div>

      <!-- Anual -->
      <div class="col-12 col-md-4">
        <div class="card h-100 shadow-lg border-0 rounded-4 plan-card">
          <div class="card-body p-4">
            <div class="d-flex align-items-center mb-2">
              <i class="bi bi-trophy fs-3 me-2"></i>
              <h4 class="mb-0">Anual</h4>
            </div>
            <h2 class="fw-bold mb-3">$120</h2>
            <ul class="list-unstyled small">
              <li><i class="bi bi-check2-circle me-1"></i>Acceso 12 meses</li>
              <li><i class="bi bi-check2-circle me-1"></i>Asesorías y retos</li>
              <li><i class="bi bi-check2-circle me-1"></i>Descuentos y sorpresas</li>
            </ul>
            <button type="button" class="btn btn-brand w-100 select-plan" data-plan="Anual">
              Elegir Anual
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Resumen y pago -->
    <div class="row mt-5 justify-content-center">
      <div class="col-12 col-lg-8">
        <div class="card border-0 shadow rounded-4" style="background:#0f0f13; color:#fff">
          <div class="card-body p-4">
            <h5 class="fw-bold mb-3"><i class="bi bi-receipt-cutoff me-2"></i>Resumen</h5>

            <div class="row g-3">
              <div class="col-md-4">
                <label class="form-label">Plan</label>
                <asp:TextBox runat="server" ID="txtPlan" CssClass="form-control" ReadOnly="true" />
              </div>

              <div class="col-md-4">
                <label class="form-label">Precio</label>
                <asp:TextBox runat="server" ID="txtPrice" CssClass="form-control" ReadOnly="true" />
              </div>

              <div class="col-md-4">
                <label class="form-label">Método de pago</label>
                <asp:DropDownList runat="server" ID="ddlMethod" CssClass="form-select">
                  <asp:ListItem Text="Seleccionar..." Value="" />
                  <asp:ListItem Text="Efectivo" Value="CASH" />
                  <asp:ListItem Text="Tarjeta" Value="CARD" />
                  <asp:ListItem Text="Transferencia" Value="TRANSFER" />
                </asp:DropDownList>
              </div>

              <div class="col-md-6">
                <label class="form-label">Inicio</label>
                <asp:TextBox runat="server" ID="txtStartDate" TextMode="Date" CssClass="form-control" />
              </div>

              <div class="col-md-6">
                <label class="form-label">Fin</label>
                <asp:TextBox runat="server" ID="txtEndDate" TextMode="Date" CssClass="form-control" ReadOnly="true" />
              </div>
            </div>

            <hr class="my-4" />




            <!-- Botón final -->
<asp:Button runat="server" ID="btnPay" Text="Pagar y activar"
            CssClass="btn btn-brand btn-lg w-100"
            OnClick="btnPay_Click" ValidationGroup="pay" />



<!-- Mensaje de éxito -->
<asp:Label runat="server" ID="lblOk"
           CssClass="alert alert-success d-block mt-3"
           Visible="false" EnableViewState="false" />

          </div>
        </div>
      </div>
    </div>
  </div>

  <script>
      const priceMap = { Mensual: 15, Trimestral: 40, Anual: 120 };

      function setPlan(plan) {
          // Inputs visibles
          document.getElementById('<%= txtPlan.ClientID %>').value = plan;
        document.getElementById('<%= txtPrice.ClientID %>').value = '$ ' + priceMap[plan].toFixed(2);

        // Campos ocultos que llegan al servidor
        document.getElementById('<%= hidPlan.ClientID %>').value = plan;
        document.getElementById('<%= hidPrice.ClientID %>').value = priceMap[plan].toFixed(2);

      // Fechas
      const start = document.getElementById('<%= txtStartDate.ClientID %>');
      const end   = document.getElementById('<%= txtEndDate.ClientID %>');
      const d0 = start.value ? new Date(start.value + 'T00:00:00') : new Date();
      const add = plan === 'Mensual' ? 1 : (plan === 'Trimestral' ? 3 : 12);
      const d1 = new Date(d0); d1.setMonth(d1.getMonth() + add);
      end.value = d1.toISOString().slice(0,10);
    }

    document.addEventListener('click', (e) => {
      const btn = e.target.closest('.select-plan');
      if (btn) setPlan(btn.dataset.plan);
    });

    document.addEventListener('DOMContentLoaded', () => {
      const sd = document.getElementById('<%= txtStartDate.ClientID %>');
        if (!sd.value) sd.value = new Date().toISOString().slice(0, 10);
    });
  </script>
</asp:Content>
