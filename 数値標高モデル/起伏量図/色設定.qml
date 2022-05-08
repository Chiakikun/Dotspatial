<!DOCTYPE qgis PUBLIC 'http://mrcc.com/qgis.dtd' 'SYSTEM'>
<qgis hasScaleBasedVisibilityFlag="0" minScale="1e+08" styleCategories="AllStyleCategories" version="3.16.13-Hannover" maxScale="0">
  <flags>
    <Identifiable>1</Identifiable>
    <Removable>1</Removable>
    <Searchable>1</Searchable>
  </flags>
  <temporal fetchMode="0" enabled="0" mode="0">
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
      <resampling maxOversampling="2" enabled="false" zoomedOutResamplingMethod="nearestNeighbour" zoomedInResamplingMethod="nearestNeighbour"/>
    </provider>
    <rasterrenderer opacity="1" type="singlebandpseudocolor" nodataColor="" alphaBand="-1" classificationMin="0" classificationMax="120" band="1">
      <rasterTransparency/>
      <minMaxOrigin>
        <limits>None</limits>
        <extent>WholeRaster</extent>
        <statAccuracy>Estimated</statAccuracy>
        <cumulativeCutLower>0.02</cumulativeCutLower>
        <cumulativeCutUpper>0.98</cumulativeCutUpper>
        <stdDevFactor>2</stdDevFactor>
      </minMaxOrigin>
      <rastershader>
        <colorrampshader labelPrecision="4" classificationMode="1" colorRampType="INTERPOLATED" clip="0" minimumValue="0" maximumValue="120">
          <colorramp type="gradient" name="[source]">
            <prop k="color1" v="5,113,176,255"/>
            <prop k="color2" v="202,0,32,255"/>
            <prop k="discrete" v="0"/>
            <prop k="rampType" v="gradient"/>
            <prop k="stops" v="0.25;146,197,222,255:0.5;247,247,247,255:0.75;244,165,130,255"/>
          </colorramp>
          <item label="0.0000" alpha="255" color="#25c5e5" value="0"/>
          <item label="30.0000" alpha="255" color="#008800" value="30"/>
          <item label="60.0000" alpha="255" color="#f3ff0a" value="60"/>
          <item label="90.0000" alpha="255" color="#ca0020" value="90"/>
          <item label="120.0000" alpha="255" color="#ffffff" value="120"/>
        </colorrampshader>
      </rastershader>
    </rasterrenderer>
    <brightnesscontrast contrast="0" gamma="1" brightness="0"/>
    <huesaturation colorizeGreen="128" grayscaleMode="0" colorizeOn="0" saturation="0" colorizeStrength="100" colorizeRed="255" colorizeBlue="128"/>
    <rasterresampler maxOversampling="2"/>
    <resamplingStage>resamplingFilter</resamplingStage>
  </pipe>
  <blendMode>0</blendMode>
</qgis>
