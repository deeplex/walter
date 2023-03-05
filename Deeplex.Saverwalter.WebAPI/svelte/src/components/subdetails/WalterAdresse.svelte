<script lang="ts">
	import { Row } from 'carbon-components-svelte';

	import type { WalterAdresseEntry } from '$WalterTypes';
	import { WalterTextInput } from '$WalterComponents';

	export let value: Partial<WalterAdresseEntry> | undefined = undefined;

	let fallback: Partial<WalterAdresseEntry> = {};

	const change = () => {
		value = fallback;
	};
</script>

<Row>
	{#if value}
		<WalterTextInput labelText="Straße" bind:value={value.strasse} />
		<WalterTextInput labelText="Hausnr." bind:value={value.hausnummer} />
		<WalterTextInput labelText="Postleitzahl" bind:value={value.postleitzahl} />
		<WalterTextInput labelText="Stadt" bind:value={value.stadt} />
	{:else}
		<WalterTextInput
			{change}
			labelText="Straße"
			bind:value={fallback.strasse}
		/>
		<WalterTextInput
			{change}
			labelText="Hausnr."
			bind:value={fallback.hausnummer}
		/>
		<WalterTextInput
			{change}
			labelText="Postleitzahl"
			bind:value={fallback.postleitzahl}
		/>
		<WalterTextInput {change} labelText="Stadt" bind:value={fallback.stadt} />
	{/if}
</Row>
