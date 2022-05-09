<!DOCTYPE qgis PUBLIC 'http://mrcc.com/qgis.dtd' 'SYSTEM'>
<qgis minScale="1e+08" styleCategories="AllStyleCategories" maxScale="0" version="3.16.13-Hannover" hasScaleBasedVisibilityFlag="0">
  <flags>
    <Identifiable>1</Identifiable>
    <Removable>1</Removable>
    <Searchable>1</Searchable>
  </flags>
  <temporal enabled="0" mode="0" fetchMode="0">
    <fixedRange>
      <start></start>
      <end></end>
    </fixedRange>
  </temporal>
  <customproperties>
    <property value="false" key="WMSBackgroundLayer"/>
    <property value="false" key="WMSPublishDataSourceUrl"/>
    <property value="0" key="embeddedWidgets/count"/>
    <property value="Value" key="identify/format"/>
  </customproperties>
  <pipe>
    <provider>
      <resampling enabled="false" zoomedOutResamplingMethod="nearestNeighbour" zoomedInResamplingMethod="nearestNeighbour" maxOversampling="2"/>
    </provider>
    <rasterrenderer band="1" opacity="1" alphaBand="-1" nodataColor="" classificationMin="0" classificationMax="256" type="singlebandpseudocolor">
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
        <colorrampshader minimumValue="0" labelPrecision="4" classificationMode="1" colorRampType="INTERPOLATED" clip="0" maximumValue="256">
          <colorramp name="[source]" type="gradient">
            <prop v="5,113,176,255" k="color1"/>
            <prop v="202,0,32,255" k="color2"/>
            <prop v="0" k="discrete"/>
            <prop v="gradient" k="rampType"/>
            <prop v="0.335337;0,255,0,255:0.665865;255,255,0,255" k="stops"/>
          </colorramp>
          <item alpha="255" label="0.0000" value="0" color="#88c0ff"/>
          <item alpha="255" label="65.0000" value="65" color="#94faff"/>
          <item alpha="255" label="130.0000" value="130" color="#00ff00"/>
          <item alpha="255" label="195.0000" value="195" color="#ffff00"/>
          <item alpha="255" label="256.0000" value="256" color="#ca0020"/>
        </colorrampshader>
      </rastershader>
    </rasterrenderer>
    <brightnesscontrast brightness="0" gamma="1" contrast="0"/>
    <huesaturation colorizeRed="255" colorizeGreen="128" colorizeStrength="100" grayscaleMode="0" colorizeOn="0" saturation="0" colorizeBlue="128"/>
    <rasterresampler maxOversampling="2"/>
    <resamplingStage>resamplingFilter</resamplingStage>
  </pipe>
  <blendMode>0</blendMode>
</qgis>
