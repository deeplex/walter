<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';
	import {
		Anhaenge,
		Kontakte,
		Mieten,
		Mietminderungen,
		WalterComboBox,
		WalterDatePicker,
		WalterGrid,
		WalterHeader
	} from '../../../components';
	import { walter_get } from '../../../services/utils';
	import type { SelectionEntry } from '../../../types/selection.type';
	import type { VertragEntry } from '../../../types/vertrag.type';
	import type { PageData } from './$types';

	export let data: PageData;

	const a: Promise<VertragEntry> = walter_get(`/api/vertraege/${data.id}`);
	const entry: Partial<VertragEntry> = {};
	a.then((e) => Object.assign(entry, e));

	// let vermieter = () =>
	// 	kontakte.then(async (e) => {
	// 		const besitzer = await async.then((e) => e.wohnung.besitzerId);
	// 		return e.find((f) => besitzer === f.id)?.text;
	// 	});
</script>

<WalterHeader
	title={a.then(
		(x) => x.wohnung.text + ' - ' + x.mieter.map((m) => m.name).join(', ')
	)}
>
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>

<WalterGrid>
	<Row>
		<WalterDatePicker
			bind:binding={entry.beginn}
			value={a.then((x) => x.beginn)}
			labelText="Beginn"
		/>
		<WalterDatePicker
			bind:binding={entry.ende}
			value={a.then((x) => x.ende)}
			labelText="Ende"
			placeholder="Offen"
		/>
	</Row>
	<Row>
		<WalterComboBox
			bind:binding={entry.wohnung}
			api={`/api/selection/kontakte`}
			value={a.then((x) => x.wohnung)}
			titleText="Wohnung"
		/>
		<!-- TODO -->
		<!-- <Column>
				<div style="margin-top:0.75rem">
					<p class="bx--label">Vermieter:</p>
					{#await vermieter()}
						<TextInputSkeleton />
					{:then y}
						<p style="margin-top: 0.5rem" class=".bx--text-input::placeholder">
							{y}
						</p>
					{/await}
				</div>
			</Column> -->
		<!-- TODO -->
		<WalterComboBox
			bind:binding={entry.ansprechpartner}
			api={`/api/selection/wohnungen`}
			value={a.then((x) => x.ansprechpartner)}
			titleText="Ansprechpartner"
		/>
	</Row>

	<Accordion>
		<Kontakte title="Mieter" rows={a.then((x) => x.mieter)} />
		<Mieten title="Mieten" rows={a.then((x) => x.mieten)} />
		<Mietminderungen
			title="Mietminderungen"
			rows={a.then((x) => x.mietminderungen)}
		/>
	</Accordion>
</WalterGrid>
