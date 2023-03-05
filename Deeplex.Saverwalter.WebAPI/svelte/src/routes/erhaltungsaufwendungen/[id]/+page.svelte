<script lang="ts">
	import type { PageData } from './$types';

	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterErhaltungsaufwendung
	} from '$WalterComponents';

	export let data: PageData;
</script>

<WalterHeaderDetail
	a={data.a}
	url={data.url}
	title={data.a.aussteller?.text + ' - ' + data.a.bezeichnung}
/>

<WalterGrid>
	<WalterErhaltungsaufwendung a={data.a} />
</WalterGrid>
