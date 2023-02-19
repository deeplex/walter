<script lang="ts">
	import { Header, Loading } from 'carbon-components-svelte';

	export let title: Promise<string> | string = 'Saverwalter';
</script>

{#await title}
	<Header company="SaverWalter	|	" platformName="Lade...">
		<Loading withOverlay={false} small />
	</Header>
{:then x}
	<Header company="SaverWalter	|	" platformName={x}>
		<slot />
	</Header>
{/await}

<!-- <svelte:fragment slot="skip-to-content">
		<SkipToContent />
	</svelte:fragment> -->
<!-- <HeaderUtilities>
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
	</HeaderUtilities> -->
