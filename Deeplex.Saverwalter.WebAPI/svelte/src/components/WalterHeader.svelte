<script lang="ts">
	import { Header, HeaderUtilities, Loading } from 'carbon-components-svelte';

	export let title: Promise<string> | string = 'Saverwalter';

	const company = 'SaverWalter | ';
</script>

{#await title}
	<Header href="/" {company} platformName="Lade...">
		<Loading withOverlay={false} small />
	</Header>
{:then x}
	<Header href="/" {company} platformName={x}>
		<HeaderUtilities>
			<slot />
		</HeaderUtilities>
	</Header>
{/await}
