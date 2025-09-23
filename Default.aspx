<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PEAKGYMM.Default" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
  <style>
    :root{
      --brand-purple: #6f42c1; /* morado atractivo */
      --brand-black: #000000;  /* negro puro */
      --brand-white: #f8f9fa;  /* blanco clarito */
    }
    .hero-wrap{
      min-height: calc(100vh - 140px); /* ocupa pantalla menos navbar/footer aprox */
      display: grid;
      place-items: center;
      background:
        radial-gradient(1200px 600px at 10% 10%, rgba(111,66,193,.15), transparent 60%),
        radial-gradient(1000px 500px at 90% 20%, rgba(111,66,193,.12), transparent 60%),
        linear-gradient(180deg, var(--brand-black), #0a0a0a);
      color: var(--brand-white);
    }
    .hero-card{
      background: rgba(255,255,255,0.03);
      border: 1px solid rgba(255,255,255,0.08);
      border-radius: 1.25rem;
      box-shadow: 0 10px 30px rgba(0,0,0,.35);
      backdrop-filter: blur(4px);
    }
    .brand-badge{
      display:inline-flex; align-items:center; gap:.5rem;
      padding:.375rem .75rem; border-radius:999px;
      background: rgba(111,66,193,.15); color:#cbb2ff; font-weight:600;
      border:1px solid rgba(111,66,193,.35);
    }
    .display-title{
      font-weight:800; letter-spacing:.5px; line-height:1.1;
      text-shadow: 0 8px 24px rgba(111,66,193,.25);
    }
    .lead-soft{ color:#d7d7dc; }
    .btn-cta{
      --bs-btn-color:#fff;
      --bs-btn-bg: var(--brand-purple);
      --bs-btn-border-color: var(--brand-purple);
      --bs-btn-hover-bg:#5a35a0; --bs-btn-hover-border-color:#5a35a0;
      --bs-btn-focus-shadow-rgb:111,66,193;
      box-shadow: 0 8px 24px rgba(111,66,193,.35);
    }
    .btn-ghost{
      color: var(--brand-white);
      border:1px solid rgba(255,255,255,.35);
      background: transparent;
    }
    .btn-ghost:hover{
      color:#fff; border-color:#fff; background: rgba(255,255,255,.06);
    }
    .tiny-note{ color:#a8a8b3; font-size:.9rem; }
  </style>

  <section class="hero-wrap">
    <div class="container">
      <div class="row justify-content-center">
        <div class="col-12 col-lg-8">
          <div class="hero-card p-4 p-md-5 text-center">
            <div class="mb-3">
              <span class="brand-badge">
                <i class="bi bi-activity"></i> PEAK GYMM
              </span>
            </div>
            <h1 class="display-4 display-title mb-3">
              Entrena inteligente. Gestiona <span style="color:var(--brand-purple)">mejor</span>.
            </h1>
            <p class="lead lead-soft mb-4">
              Accede a tus rutinas y progreso, o administra socios y membresías.
            </p>

            <div class="d-grid gap-3 d-sm-flex justify-content-center">
              <a href="~Login.aspx" class="btn btn-cta btn-lg px-4">
                <i class="bi bi-box-arrow-in-right me-2"></i> Iniciar sesión
              </a>
              <a href="~Register.aspx" class="btn btn-ghost btn-lg px-4">
                <i class="bi bi-person-plus me-2"></i> Registrarme
              </a>
            </div>

            <div class="mt-4 tiny-note">
              ¿Ya tienes sesión iniciada? Te enviaremos automáticamente a tu panel.
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</asp:Content>
