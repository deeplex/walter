<script lang="ts">
	import { Header, HeaderUtilities, Loading } from 'carbon-components-svelte';

	export let title: Promise<string> | string = 'Saverwalter';

	const company = 'SaverWalter | ';
</script>

{#await title}
	<Header {company} platformName="Lade...">
		<Loading withOverlay={false} small />
	</Header>
{:then x}
	<Header {company} platformName={x}>
		<HeaderUtilities>
			<slot />
		</HeaderUtilities>
	</Header>
{/await}

<!-- <svelte:fragment slot="skip-to-content">
		<SkipToContent />
	</svelte:fragment> -->
<!-- <HeaderUtilities>

	</HeaderUtilities> -->
