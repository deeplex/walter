<script>
	import 'carbon-components-svelte/css/white.css';
	import {
		Header,
		HeaderNav,
		HeaderNavItem,
		HeaderNavMenu,
		SideNav,
		SideNavItems,
		SideNavMenu,
		SideNavMenuItem,
		SideNavLink,
		SideNavDivider,
		SkipToContent,
		Content,
		Grid,
		Row,
		Column,
		HeaderPanelLinks,
		HeaderAction,
		HeaderUtilities,
		HeaderPanelDivider,
		HeaderPanelLink
	} from 'carbon-components-svelte';
	import { UserMultiple, Building, Document, Money } from 'carbon-icons-svelte/lib';

	let isSideNavOpen = true;
</script>

<Header persistentHamburgerMenu={true} platformName="Saverwalter" bind:isSideNavOpen>
	<svelte:fragment slot="skip-to-content">
		<SkipToContent />
	</svelte:fragment>
	<HeaderUtilities>
		<HeaderNav>
			<HeaderNavItem href="/" text="Link 1" />
			<HeaderNavItem href="/" text="Link 2" />
			<HeaderNavItem href="/" text="Link 3" />
			<HeaderNavMenu text="Menu">
				<HeaderNavItem href="/" text="Link 1" />
				<HeaderNavItem href="/" text="Link 2" />
				<HeaderNavItem href="/" text="Link 3" />
			</HeaderNavMenu>
			<HeaderNavItem href="/" text="Link 4" />
		</HeaderNav>
		<HeaderAction>
			<HeaderPanelLinks>
				<HeaderPanelDivider>Switcher subject 1</HeaderPanelDivider>
				<HeaderPanelLink>Switcher item 1</HeaderPanelLink>
				<HeaderPanelDivider>Switcher subject 2</HeaderPanelDivider>
				<HeaderPanelLink>Switcher item 1</HeaderPanelLink>
				<HeaderPanelLink>Switcher item 2</HeaderPanelLink>
				<HeaderPanelLink>Switcher item 3</HeaderPanelLink>
				<HeaderPanelLink>Switcher item 4</HeaderPanelLink>
				<HeaderPanelLink>Switcher item 5</HeaderPanelLink>
			</HeaderPanelLinks>
		</HeaderAction>
	</HeaderUtilities>
</Header>

<SideNav bind:isOpen={isSideNavOpen}>
	<SideNavItems>
		<SideNavLink icon={UserMultiple} text="Kontakte" href="/kontakte" />
		<SideNavLink icon={Building} text="Wohnungen" href="/wohnungen" />
		<SideNavLink icon={Document} text="Verträge" href="/vertraege" />
		<SideNavMenu icon={Money} text="Rechnungen" href="/rechnungen">
			<SideNavMenuItem text="Betriebskosten" href="/rechnungen/betriebskosten" />
			<SideNavMenuItem text="Erhaltungen" href="/rechnungen/erhaltungen" />
		</SideNavMenu>
		<SideNavDivider />
	</SideNavItems>
</SideNav>

<Content>
	<main>
		<slot />
	</main>
</Content>

<style>
	slot {
		height: 100vh - 3rem - 5px;
		width: 100vw;
		position: relative;
	}

	:global(.bx--text-input-wrapper, .bx--form-item, .bx--date-picker-container) {
		margin: 10px;
	}
	:global(.bx--content) {
		padding: 0;
	}
	:global(.bx--data-table--sticky-header) {
		max-height: calc(100vh - 3rem - 5px);
	}
</style>
