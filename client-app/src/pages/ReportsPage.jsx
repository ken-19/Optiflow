import { useState } from 'react';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, ArcElement, PointElement, LineElement, Title, Tooltip, Legend, Filler } from 'chart.js';
import { Bar, Doughnut, Line } from 'react-chartjs-2';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, PointElement, LineElement, Title, Tooltip, Legend, Filler);

const chartOpts = { responsive: true, maintainAspectRatio: false, plugins: { legend: { labels: { color: '#94a3b8' } } }, scales: { x: { ticks: { color: '#64748b' }, grid: { color: '#2a2f45' } }, y: { ticks: { color: '#64748b' }, grid: { color: '#2a2f45' } } } };
const dOpts = { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom', labels: { color: '#94a3b8', padding: 16 } } } };

const reports = [
  { id: 1, name: 'Monthly Production Summary', type: 'Production', date: '2026-04-01', status: 'Generated' },
  { id: 2, name: 'Q1 Cost Analysis', type: 'Costing', date: '2026-03-31', status: 'Generated' },
  { id: 3, name: 'Inventory Audit Report', type: 'Inventory', date: '2026-04-15', status: 'Generated' },
  { id: 4, name: 'Quality Metrics Q1', type: 'Quality', date: '2026-03-31', status: 'Generated' },
  { id: 5, name: 'Defect Trend Analysis', type: 'Quality', date: '2026-04-20', status: 'Pending' },
  { id: 6, name: 'Material Usage Report', type: 'Inventory', date: '2026-04-25', status: 'Generated' },
];

export default function ReportsPage() {
  const [tab, setTab] = useState('overview');
  const typeBadge = (t) => { const m = { Production: 'badge-purple', Costing: 'badge-warning', Inventory: 'badge-info', Quality: 'badge-success' }; return <span className={`badge ${m[t]||'badge-info'}`}>{t}</span>; };

  const handleDownload = (report) => {
    if (report.status !== 'Generated') {
      alert('Report is still pending generation.');
      return;
    }

    const doc = new jsPDF();
    
    // Header
    doc.setFillColor(30, 34, 53);
    doc.rect(0, 0, 210, 40, 'F');
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(22);
    doc.text('OptiFlow ERP-MES', 14, 25);
    
    doc.setFontSize(10);
    doc.text(`Generated on: ${new Date().toLocaleString()}`, 140, 25);

    // Body
    doc.setTextColor(0, 0, 0);
    doc.setFontSize(18);
    doc.text(report.name, 14, 55);
    
    doc.setFontSize(12);
    doc.text(`Report Type: ${report.type}`, 14, 65);
    doc.text(`Base Date: ${report.date}`, 14, 72);
    doc.text(`Report ID: OPTI-${report.id.toString().padStart(4, '0')}`, 14, 79);

    // Mock Data Table
    const tableData = [
      ['Metric', 'Value', 'Status'],
      ['Total Cycles', '1,240', 'Completed'],
      ['Average Lead Time', '4.2 Days', 'Within Target'],
      ['Resource Utilization', '88.5%', 'High'],
      ['Error Rate', '0.4%', 'Low']
    ];

    autoTable(doc, {
      startY: 90,
      head: [['Parameter', 'Measurement', 'Notes']],
      body: [
        ['Total Units Processed', '5,420', 'All verified'],
        ['Material Waste', '2.1%', 'Trend: Down'],
        ['Machine Downtime', '12.4 hrs', 'Maintenance scheduled'],
        ['Quality Pass Rate', '97.2%', 'Target exceeded'],
      ],
      theme: 'grid',
      headStyles: { fillColor: [99, 102, 241] }
    });

    // Footer
    const pageCount = doc.internal.getNumberOfPages();
    for(let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      doc.setFontSize(10);
      doc.setTextColor(150);
      doc.text(`Page ${i} of ${pageCount}`, 14, 285);
      doc.text('Confidential - OptiFlow Manufacturing Inc.', 130, 285);
    }

    doc.save(`${report.name.replace(/\s+/g, '_')}.pdf`);
  };

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon purple">📈</div><div className="stat-value">{reports.length}</div><div className="stat-label">Total Reports</div></div>
        <div className="card stat-card"><div className="stat-icon green">✅</div><div className="stat-value">{reports.filter(r=>r.status==='Generated').length}</div><div className="stat-label">Generated</div></div>
        <div className="card stat-card"><div className="stat-icon amber">⏳</div><div className="stat-value">{reports.filter(r=>r.status==='Pending').length}</div><div className="stat-label">Pending</div></div>
      </div>
      <div className="toolbar">
        <div style={{display:'flex',gap:8}}>
          <button className={`btn ${tab==='overview'?'btn-primary':'btn-ghost'}`} onClick={()=>setTab('overview')}>📊 Charts</button>
          <button className={`btn ${tab==='list'?'btn-primary':'btn-ghost'}`} onClick={()=>setTab('list')}>📋 Report List</button>
        </div>
        <button className="btn btn-primary">+ Generate Report</button>
      </div>
      {tab === 'overview' ? (
        <div className="charts-grid">
          <div className="card chart-card"><div className="card-header"><span className="card-title">Production Efficiency (Weekly)</span></div><div style={{height:260}}><Line data={{labels:['W1','W2','W3','W4','W5','W6'],datasets:[{label:'Efficiency %',data:[85,88,82,91,87,93],borderColor:'#6366f1',backgroundColor:'rgba(99,102,241,0.2)',fill:true,tension:0.4},{label:'Target',data:[90,90,90,90,90,90],borderColor:'#64748b',borderDash:[5,5],pointRadius:0}]}} options={chartOpts} /></div></div>
          <div className="card chart-card"><div className="card-header"><span className="card-title">Cost Breakdown</span></div><div style={{height:260}}><Doughnut data={{labels:['Labor','Material','Overhead','Equipment'],datasets:[{data:[35,42,15,8],backgroundColor:['#6366f1','#22c55e','#f59e0b','#3b82f6'],borderWidth:0}]}} options={dOpts} /></div></div>
          <div className="card chart-card"><div className="card-header"><span className="card-title">Monthly Output</span></div><div style={{height:260}}><Bar data={{labels:['Jan','Feb','Mar','Apr'],datasets:[{label:'Units Produced',data:[450,520,480,610],backgroundColor:'#6366f1',borderRadius:6},{label:'Target',data:[500,500,500,500],backgroundColor:'#2a2f45',borderRadius:6}]}} options={chartOpts} /></div></div>
          <div className="card chart-card"><div className="card-header"><span className="card-title">Quality Trend</span></div><div style={{height:260}}><Line data={{labels:['Jan','Feb','Mar','Apr'],datasets:[{label:'Pass Rate %',data:[94,96,93,97],borderColor:'#22c55e',backgroundColor:'rgba(34,197,94,0.2)',fill:true,tension:0.4},{label:'Defect Rate %',data:[6,4,7,3],borderColor:'#ef4444',backgroundColor:'rgba(239,68,68,0.1)',fill:true,tension:0.4}]}} options={chartOpts} /></div></div>
        </div>
      ) : (
        <div className="card">
          <div className="table-container" style={{border:'none'}}>
            <table className="data-table">
              <thead><tr><th>Report Name</th><th>Type</th><th>Date</th><th>Status</th><th>Action</th></tr></thead>
              <tbody>{reports.map(r => (
                <tr key={r.id}><td style={{fontWeight:600}}>{r.name}</td><td>{typeBadge(r.type)}</td><td>{r.date}</td><td>{r.status==='Generated'?<span className="badge badge-success">Generated</span>:<span className="badge badge-warning">Pending</span>}</td><td><button className="btn btn-ghost btn-sm" onClick={() => handleDownload(r)}>📥 Download</button></td></tr>
              ))}</tbody>
            </table>
          </div>
        </div>
      )}
    </>
  );
}
