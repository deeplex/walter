<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

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
		Location,
		Logout,
		Meter,
		Money,
		Settings,
		Tools,
		UserMultiple
	} from 'carbon-icons-svelte';
	import Cookies from 'js-cookie';

	function logout() {
		Cookies.remove('access_token');
		goto('/login');
	}
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
			isSelected={$page.route.id?.includes('/adressen')}
			icon={Location}
			text="Adressen"
			href="/adressen"
		/>

		<SideNavDivider />

		<SideNavMenu text="Einstellungen" icon={Settings}>
			<SideNavLink
				icon={Logout}
				on:click={logout}
				text="Abmelden"
				style="padding-left: 4em"
			/>
		</SideNavMenu>
	</SideNavItems>
</SideNav>
