<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:frmwrk="Corel Framework Data">
  <xsl:output method="xml" encoding="UTF-8" indent="yes"/>

  <frmwrk:uiconfig>
    <frmwrk:applicationInfo userConfiguration="true" />
  </frmwrk:uiconfig>

  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="uiConfig/items">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>

      <itemData guid="7e4b2c1a-5d8f-49e3-b6c0-2a8d1f4e7b9c"
                type="wpfhost"
                hostedType="Addons\QiTuCDR\QiTuCDR.dll,QiTuCDR.AddonEntry"
                enable="true">
      </itemData>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="uiConfig/commandBars">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>

      <commandBarData guid="d6a8f3e1-9b2c-4d5a-8e7f-1c3a5b7d9e0f"
                      nonLocalizableName="QiTuCDR"
                      userCaption="企图插件"
                      locked="false"
                      type="toolbar">
        <toolbar>
          <item guidRef="7e4b2c1a-5d8f-49e3-b6c0-2a8d1f4e7b9c" dock="top"/>
        </toolbar>
      </commandBarData>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="uiConfig/containers/container[@guid='bee85f91-3ad9-dc8d-48b5-d2a87c8b2109']/container[@guid='Framework_MainFrame-layout']/dockHost[@guid='894bf987-2ec1-8f83-41d8-68f6797d0db4']/toolbar[@guidRef='c2b44f69-6dec-444e-a37e-5dbf7ff43dae']">
    <xsl:copy-of select="."/>
    <toolbar guidRef="d6a8f3e1-9b2c-4d5a-8e7f-1c3a5b7d9e0f" dock="top" />
  </xsl:template>

</xsl:stylesheet>
