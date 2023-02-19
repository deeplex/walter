<script lang="ts">
	import {
		SideNav,
		SideNavDivider,
		SideNavItems,
		SideNavLink,
		SideNavMenu,
		SideNavMenuItem
	} from 'carbon-components-svelte';
	import {
		Building,
		ChartRelationship,
		Document,
		DocumentAttachment,
		Meter,
		Money,
		Tools,
		UserMultiple
	} from 'carbon-icons-svelte';
</script>

<SideNav rail>
	<SideNavItems>
		<SideNavLink icon={UserMultiple} text="Kontakte" href="/kontakte" />
		<SideNavLink icon={Building} text="Wohnungen" href="/wohnungen" />
		<SideNavLink icon={Document} text="Verträge" href="/vertraege" />
		<SideNavLink
			icon={Money}
			text="Betriebskostenrechnungen"
			href="/betriebskostenrechnungen"
		/>
		<SideNavLink
			icon={Tools}
			text="Erhaltungsaufwendungen"
			href="/erhaltungsaufwendungen"
		/>
		<SideNavLink icon={ChartRelationship} text="Umlagen" href="/umlagen" />
		<SideNavLink icon={Meter} text="Zähler" href="/zaehler" />
		<SideNavLink icon={DocumentAttachment} text="Anhänge" href="/anhaenge" />
		<SideNavDivider />
	</SideNavItems>
</SideNav>
