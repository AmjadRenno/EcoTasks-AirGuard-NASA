window.airGuardMap = {
  init: function (elemId, lat, lon, zoom) {
    const map = L.map(elemId).setView([lat, lon], zoom || 4);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 18,
      attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    const markersLayer = L.layerGroup().addTo(map);
    map._airGuard = { markersLayer };

    return map;
  },

  clearMarkers: function (map) {
    if (!map || !map._airGuard || !map._airGuard.markersLayer) return;
    map._airGuard.markersLayer.clearLayers();
  },

  addCityMarker: function (map, city, aqi, lat, lon) {
    const color =
      aqi <= 50 ? '#00E676' :
      aqi <= 100 ? '#FFD600' :
      aqi <= 150 ? '#FF9100' :
      aqi <= 200 ? '#F44336' :
      aqi <= 300 ? '#8E24AA' : '#6D4C41';

    const marker = L.circleMarker([lat, lon], {
      radius: 10, color, fillOpacity: 0.8
    });

    const layer = map && map._airGuard ? map._airGuard.markersLayer : null;
    if (layer) {
      layer.addLayer(marker);
    } else {
      marker.addTo(map);
    }

    marker.bindPopup(`<b>${city}</b><br/>AQI: ${aqi}`);
  },

  addColoredCityMarker: function (map, city, aqi, lat, lon, color, category, riskType) {
    const riskIcon = this.getRiskIcon(riskType);
    
    const marker = L.circleMarker([lat, lon], {
      radius: Math.max(8, Math.min(16, aqi / 8)),
      color: '#1F1F1F',
      fillColor: color,
      fillOpacity: 0.92,
      weight: 2
    });

    const layer = map && map._airGuard ? map._airGuard.markersLayer : null;
    if (layer) {
      layer.addLayer(marker);
    } else {
      marker.addTo(map);
    }

    // Determine display category
    let displayCategory = category;
    if (!category || category === 'Unknown') {
      if (aqi <= 50) displayCategory = 'Good';
      else if (aqi <= 100) displayCategory = 'Moderate';
      else if (aqi <= 150) displayCategory = 'Unhealthy for Sensitive Groups';
      else if (aqi <= 200) displayCategory = 'Unhealthy';
      else if (aqi <= 300) displayCategory = 'Very Unhealthy';
      else displayCategory = 'Hazardous';
    }

    const popupContent = `
      <div style="min-width: 200px;">
        <h6 style="margin: 0 0 8px 0; color: #333;">${riskIcon} ${city}</h6>
        <div style="margin-bottom: 10px; display: flex; align-items: baseline; gap: 10px;">
          <span style="font-size: 28px; font-weight: 700; color: ${color};">${Math.round(aqi)}</span>
          <span style="color: #555; font-weight: 600;">${displayCategory}</span>
        </div>
        <div style="display: flex; justify-content: flex-end;">
          <button onclick="window.open('/dashboard?city=${encodeURIComponent(city)}', '_blank')" 
                  style="background: #007bff; color: white; border: none; padding: 6px 14px; border-radius: 4px; cursor: pointer; font-size: 0.85rem;">
            View Details
          </button>
        </div>
      </div>
    `;

    marker.bindPopup(popupContent);
  },

  getHealthAdvice: function(aqi) {
    if (aqi <= 50) return "Air quality is good. Ideal for outdoor activities.";
    if (aqi <= 100) return "Air quality is acceptable; consider limiting prolonged exposure if sensitive.";
    if (aqi <= 150) return "Sensitive groups should limit outdoor exposure.";
    if (aqi <= 200) return "Everyone should reduce outdoor activities.";
    if (aqi <= 300) return "Health alert: everyone may experience health effects.";
    return "Emergency conditions: all outdoor activities should be avoided.";
  },

  getRiskIcon: function(riskType) {
    switch(riskType) {
      case 'wildfire': return '🔥';
      case 'ozone': return '☀️';
      case 'dust': return '🌪️';
      default: return '🏙️';
    }
  }
};