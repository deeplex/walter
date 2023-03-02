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

	const url = `/api/kontakte`;
	const title = 'Neue Person';

	const entry: Partial<
		WalterNatuerlichePersonEntry & WalterJuristischePersonEntry
	> = {};

	let personType: number = 0;
</script>

<WalterHeaderNew url={url + `/${personType ? 'jur' : 'nat'}`} {title} {entry}>
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
			<WalterTextInput bind:binding={entry.vorname} labelText="Vorname" />
			<WalterTextInput bind:binding={entry.nachname} labelText="Nachname" />
		{:else}
			<WalterTextInput bind:binding={entry.name} labelText="Bezeichnung" />
		{/if}
	</Row>
	<WalterPerson binding={entry} />
</WalterGrid>
