<script lang="ts">
	import { ContentSwitcher, Row, Switch } from 'carbon-components-svelte';

	import type {
		WalterJuristischePersonEntry,
		WalterNatuerlichePersonEntry
	} from '$WalterTypes';
	import {
		WalterPerson,
		WalterGrid,
		WalterTextInput,
		WalterHeaderNew
	} from '$WalterComponents';

	const apiURL = `/api/kontakte`;
	const title = 'Neue Person';

	const entry: Partial<
		WalterNatuerlichePersonEntry & WalterJuristischePersonEntry
	> = {};

	let personType: number = 0;
</script>

<WalterHeaderNew
	apiURL={apiURL + `/${personType ? 'jur' : 'nat'}`}
	{title}
	{entry}
>
	<div style="width: 100%;">
		<ContentSwitcher
			style="display: flex; width: 60em; margin: auto"
			size="xl"
			bind:selectedIndex={personType}
		>
			<Switch style="width: 30em" text="Natürliche Person" />
			<Switch style="width: 30em" text="Juristische Person" />
		</ContentSwitcher>
	</div>
</WalterHeaderNew>

<WalterGrid>
	<Row>
		{#if personType === 0}
			<WalterTextInput bind:value={entry.vorname} labelText="Vorname" />
			<WalterTextInput bind:value={entry.nachname} labelText="Nachname" />
		{:else}
			<WalterTextInput bind:value={entry.name} labelText="Bezeichnung" />
		{/if}
	</Row>
	<WalterPerson value={entry} />
</WalterGrid>
