<!DOCTYPE qgis PUBLIC 'http://mrcc.com/qgis.dtd' 'SYSTEM'>
<qgis maxScale="0" hasScaleBasedVisibilityFlag="0" minScale="1e+08" styleCategories="AllStyleCategories" version="3.16.13-Hannover">
  <flags>
    <Identifiable>1</Identifiable>
    <Removable>1</Removable>
    <Searchable>1</Searchable>
  </flags>
  <temporal mode="0" fetchMode="0" enabled="0">
    <fixedRange>
      <start></start>
      <end></end>
    </fixedRange>
  </temporal>
  <customproperties>
    <property key="WMSBackgroundLayer" value="false"/>
    <property key="WMSPublishDataSourceUrl" value="false"/>
    <property key="embeddedWidgets/count" value="0"/>
    <property key="identify/format" value="Value"/>
  </customproperties>
  <pipe>
    <provider>
      <resampling zoomedInResamplingMethod="nearestNeighbour" zoomedOutResamplingMethod="nearestNeighbour" maxOversampling="2" enabled="false"/>
    </provider>
    <rasterrenderer classificationMax="270" opacity="1" alphaBand="-1" nodataColor="" band="1" classificationMin="-100" type="singlebandpseudocolor">
      <rasterTransparency/>
      <minMaxOrigin>
        <limits>MinMax</limits>
        <extent>WholeRaster</extent>
        <statAccuracy>Estimated</statAccuracy>
        <cumulativeCutLower>0.02</cumulativeCutLower>
        <cumulativeCutUpper>0.98</cumulativeCutUpper>
        <stdDevFactor>2</stdDevFactor>
      </minMaxOrigin>
      <rastershader>
        <colorrampshader maximumValue="270" clip="0" minimumValue="-100" labelPrecision="4" colorRampType="INTERPOLATED" classificationMode="1">
          <colorramp name="[source]" type="gradient">
            <prop v="0,0,255,255" k="color1"/>
            <prop v="215,25,28,255" k="color2"/>
            <prop v="0" k="discrete"/>
            <prop v="gradient" k="rampType"/>
            <prop v="0.125;255,0,255,255:0.25;255,0,0,255:0.375;255,153,0,255:0.5;255,255,0,255:0.625;0,255,0,255:0.75;0,243,255,255:0.875;0,136,255,255:0.997596;0,0,255,255" k="stops"/>
          </colorramp>
          <item color="#0000ff" alpha="0" value="-100" label="-100.0000"/>
          <item color="#0000ff" alpha="255" value="-90" label="-90.0000"/>
          <item color="#ff00cf" alpha="255" value="-45" label="-45.0000"/>
          <item color="#ff1800" alpha="255" value="0" label="0.0000"/>
          <item color="#ffa700" alpha="255" value="45" label="45.0000"/>
          <item color="#e4ff00" alpha="255" value="90" label="90.0000"/>
          <item color="#00fe14" alpha="255" value="135" label="135.0000"/>
          <item color="#00edff" alpha="255" value="180" label="180.0000"/>
          <item color="#0084ff" alpha="255" value="225" label="225.0000"/>
          <item color="#0000ff" alpha="255" value="270" label="270.0000"/>
        </colorrampshader>
      </rastershader>
    </rasterrenderer>
    <brightnesscontrast gamma="1" contrast="0" brightness="0"/>
    <huesaturation colorizeOn="0" colorizeStrength="100" colorizeRed="255" colorizeBlue="128" saturation="0" grayscaleMode="0" colorizeGreen="128"/>
    <rasterresampler maxOversampling="2"/>
    <resamplingStage>resamplingFilter</resamplingStage>
  </pipe>
  <blendMode>0</blendMode>
</qgis>
