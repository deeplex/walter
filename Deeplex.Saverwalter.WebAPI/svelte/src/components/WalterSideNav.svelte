<script lang="ts">
	import { page } from '$app/stores';

	import {
		SideNav,
		SideNavDivider,
		SideNavItems,
		SideNavLink
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
		<SideNavLink
			isSelected={$page.route.id?.includes('/kontakte')}
			icon={UserMultiple}
			text="Kontakte"
			href="/kontakte"
		/>

		<SideNavLink
			isSelected={$page.route.id?.includes('/wohnungen')}
			icon={Building}
			text="Wohnungen"
			href="/wohnungen"
		/>
		<SideNavLink
			isSelected={$page.route.id?.includes('/vertraege')}
			icon={Document}
			text="Verträge"
			href="/vertraege"
		/>
		<SideNavLink
			isSelected={$page.route.id?.includes('/betriebskostenrechnungen')}
			icon={Money}
			text="Betriebskostenrechnungen"
			href="/betriebskostenrechnungen"
		/>
		<SideNavLink
			isSelected={$page.route.id?.includes('/erhaltungsaufwendungen')}
			icon={Tools}
			text="Erhaltungsaufwendungen"
			href="/erhaltungsaufwendungen"
		/>
		<SideNavLink
			isSelected={$page.route.id?.includes('/umlagen')}
			icon={ChartRelationship}
			text="Umlagen"
			href="/umlagen"
		/>
		<SideNavLink
			isSelected={$page.route.id?.includes('/zaehler')}
			icon={Meter}
			text="Zähler"
			href="/zaehler"
		/>
		<SideNavLink
			isSelected={$page.route.id?.includes('/anhaenge')}
			icon={DocumentAttachment}
			text="Anhänge"
			href="/anhaenge"
		/>
		<SideNavDivider />
	</SideNavItems>
</SideNav>
